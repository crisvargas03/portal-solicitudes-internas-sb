# ADR-0034: Persistencia en archivo de texto para el catálogo de Entidades Gubernamentales

**Estado:** Aceptada (amendada 2026-09-14: formato de archivo — ver nota al final de "Decisión")
**Fecha:** 2026-09-14

## Contexto

El requerimiento original de este catálogo (mantenimiento CRUD del listado de entidades gubernamentales de República Dominicana) exige explícitamente que la persistencia sea un **archivo de texto plano** en un directorio del proyecto, no la base de datos relacional (SQL Server) que usa el resto del sistema. Es una excepción deliberada y acotada a esta única entidad — ninguna otra parte del proyecto cambia de estrategia de persistencia.

Las preguntas a resolver: formato de serialización del archivo, cómo evitar corrupción de datos ante escrituras concurrentes, y cómo encajar esto en el patrón `IRepositorioBase<T>`/`IUnitOfWork` que ya usa el resto de la Api (ver ADR-0009), dado que ese patrón está fuertemente atado a EF Core (`DbContext`, `ChangeTracker`, transacciones).

Los campos del catálogo (`Nombre`, `Categoria`, `PoderDelEstado`, `Sector`) se tomaron de `docs/ListaEntidadesGubernamentales.xlsx`, el archivo de datos real provisto — no se inventaron campos adicionales (Siglas, Dirección, Teléfono, etc.) que no están en la fuente.

## Decisión

1. **Formato: texto plano delimitado por `|` (`.txt`)**, un registro por línea (`Id|Nombre|Categoria|PoderDelEstado|Sector|Activo`), UTF-8. Decisión amendada el 2026-09-14: la versión original de este ADR eligió JSON Lines (`.jsonl`) por evitar el escapado de comas de CSV; se cambió a texto delimitado por `|` a pedido explícito de que el archivo fuera texto plano en sentido literal, no una serialización estructurada. El separador `|` no aparece en los datos reales (`docs/ListaEntidadesGubernamentales.xlsx`), a diferencia de la coma; y en vez de implementar un mecanismo de escape, los validadores de `CrearEntidadGubernamentalCommand`/`ActualizarEntidadGubernamentalCommand` (Application) prohíben el caracter `|` (`EntidadGubernamental.SEPARADOR_CAMPO_ARCHIVO`) en los campos de texto, así que una línea con exactamente 6 campos separados por `|` es siempre válida de interpretar sin ambigüedad.

2. **Estrategia de lectura/escritura: leer-todo/reescribir-todo.** Cada operación de escritura (`CrearAsync`, `ActualizarAsync`) lee el archivo completo, aplica el cambio en memoria, y reescribe el archivo completo a un archivo temporal (`<archivo>.tmp`) que luego reemplaza al archivo real con `File.Move(..., overwrite: true)` — una operación atómica a nivel de sistema de archivos, de modo que un corte a mitad de escritura nunca deja el archivo real corrupto o a medio escribir.

3. **Concurrencia: `SemaphoreSlim(1, 1)` por instancia del repositorio**, serializando toda lectura y escritura. La instancia se registra como **Singleton** en el contenedor de dependencias (no `Scoped`, a diferencia del resto de repositorios) — el semáforo debe ser único para todo el proceso, no uno nuevo por petición, o dos peticiones concurrentes podrían pisarse la escritura una a la otra sin coordinación real.

4. **No se integra a `IUnitOfWork`.** Se define una interfaz propia, `IEntidadGubernamentalRepository` (en `Application/Abstractions/Persistence`, junto al resto de interfaces de repositorio), inyectada directamente en los handlers en vez de expuesta como una propiedad más de `IUnitOfWork`. Cada método de la interfaz persiste su propio cambio de inmediato: no hay un paso de `GuardarCambiosAsync` separado, porque no existe una transacción de EF Core que confirmar. Forzarla dentro de `IUnitOfWork` — cuya interfaz ya expone `IniciarTransaccionAsync`/`ConfirmarTransaccionAsync`/`RevertirTransaccionAsync`, conceptos que no aplican a un archivo — hubiera sido más confuso que mantenerla aparte.

5. **`EntidadGubernamental` no hereda de `EntidadBase`/`CatalogoBase`.** Esas bases documentan explícitamente que "la identidad la asigna la base de datos" y que representan catálogos EF Core (Área, Prioridad, TipoSolicitud, EstadoSolicitud). Aquí el `Id` lo asigna el propio repositorio de archivo (máximo `Id` existente + 1), así que se declara como una clase independiente con la misma forma (`Id`, `Nombre`, `Activo`) más los campos propios, para no heredar una semántica que ya no es cierta.

6. **Baja lógica, no física**, igual que el resto de catálogos (`Activo`): no hay endpoint de `DELETE`, solo `PATCH` para desactivar.

## Consecuencias

- **A favor:** cumple el requerimiento explícito de persistencia en archivo sin forzar esa restricción sobre el resto del proyecto; la estrategia leer-todo/reescribir-todo es simple de razonar y correcta para un catálogo de este tamaño (~180 registros); el reemplazo atómico de archivo descarta la clase de bug más común en persistencia basada en archivos (escritura parcial visible a otro lector).
- **En contra / trade-offs:** cada escritura reserializa el catálogo completo — no escala a volúmenes grandes, pero es aceptable para un catálogo de cientos de filas, no de miles; el `SemaphoreSlim` serializa todas las lecturas y escrituras entre sí (incluso dos lecturas concurrentes esperan su turno), lo que sacrifica algo de paralelismo por simplicidad; al no compartir `IUnitOfWork`, un caso de uso futuro que necesitara modificar una `EntidadGubernamental` y una entidad EF Core en la misma operación atómica no tendría una transacción común entre ambos repositorios — no existe hoy ese caso, así que no se diseñó para él.
- **Alternativas descartadas:** JSON Lines — decisión original de este ADR, descartada porque, aunque técnicamente es un archivo de texto, no se ajustaba a lo que se pedía como "texto plano"; CSV (descartado por el problema de escapado con comas/acentos en los nombres, el mismo motivo por el que se prefirió `|` sobre `,` como separador); bloqueo de archivo a nivel de sistema operativo (`FileShare.None` + reintentos) en vez de un semáforo en memoria — descartado porque el semáforo ya resuelve el caso real (un único proceso de la Api accediendo al archivo) sin la complejidad de manejar reintentos y `IOException` de archivos bloqueados; forzar `IEntidadGubernamentalRepository` dentro de `IUnitOfWork` con métodos de transacción que no hacen nada — descartado por engañoso: un método `IniciarTransaccionAsync` que no abre ninguna transacción real es peor que no tener la interfaz.

## Carga inicial de datos

`SB.PortalSolicitudes.API/Data/entidades-gubernamentales.txt` se pobló con las 181 entidades de `docs/ListaEntidadesGubernamentales.xlsx` (Id secuencial 1-181 en el orden del archivo original, `Activo = True`), generado una única vez a partir del Excel fuente. No hay un endpoint ni un job de importación — es un archivo de datos versionado, igual que una migración de EF Core inicial lo sería para un catálogo relacional.
