# ADR-0020: CRUD de catálogos administrables (Área, Prioridad, TipoSolicitud) sin abstracción genérica

**Estado:** Aceptada
**Fecha:** 2026-09-12

## Contexto

El rol Administrador necesita mantenimiento completo (alta, edición, listado con inactivos, desactivación) de los tres catálogos genuinamente administrables: Área, Prioridad y TipoSolicitud. Estados queda fuera — es un flujo fijo de 6 estados sin edición (ver ADR-0004/0005), confirmado explícitamente para este trabajo.

`ICatalogoRepository<TCatalogo>` (ADR-0009) ya da a los tres catálogos `ObtenerActivosAsync`, `ObtenerTodosAsync` (heredado de `IRepositorioBase<T>`), `ExisteNombreAsync`, `AgregarAsync`, `Actualizar` — la persistencia no necesitó ningún método nuevo. La pregunta era solo en la capa de Application: ¿un comando/consulta genérico parametrizado por tipo de catálogo, o seis pares comando/consulta duplicados (uno por catálogo, por operación)?

Los tres catálogos casi comparten forma (`Nombre` + `Activo`), pero no del todo: Prioridad agrega `Nivel` (entero, para ordenar por urgencia) y TipoSolicitud agrega `Descripcion` (texto libre opcional). El lado de consulta ya existente (`ObtenerAreasActivas`, `ObtenerPrioridadesActivas`, `ObtenerTiposSolicitudActivos`) ya duplica una clase por catálogo en vez de generalizar, a pesar de que hoy en día son casi idénticas.

## Decisión

Se duplica el comando/consulta por catálogo: `CrearArea`/`ActualizarArea`, `CrearPrioridad`/`ActualizarPrioridad`, `CrearTipoSolicitud`/`ActualizarTipoSolicitud`, cada uno con su propio DTO (`AreaAdminDto`, `PrioridadAdminDto`, `TipoSolicitudAdminDto` en `Application/Features/Catalogos/Dtos`) y su propia consulta `Obtener<Catalogo>Todas`/`Todos` para listar con inactivos. No se introduce un `CrearCatalogoCommand<TCatalogo>` genérico.

## Consecuencias

- **A favor:** consistente con el patrón ya establecido en las consultas de catálogo activo; cada catálogo puede evolucionar su forma (agregar un campo, una regla de validación propia) sin tocar a los otros dos ni introducir casos especiales dentro de un genérico; el validador de FluentValidation de cada comando queda simple y específico (p. ej. `Nivel > 0` solo aplica a Prioridad).
- **En contra / trade-offs:** más archivos — 3 catálogos × 2 operaciones × 3 archivos (comando, handler, validador) más las consultas de listado, la mayoría con el mismo esqueleto (buscar por Id, verificar nombre duplicado, actualizar, guardar). Un cambio transversal (por ejemplo, agregar un campo `Codigo` a los tres catálogos) requiere tocar seis lugares en vez de uno.
- **Alternativas descartadas:** un `IComandoCatalogoHandler<TCatalogo, TDto>` genérico con hooks virtuales para los campos específicos de cada catálogo — descartada porque el genérico terminaría con tantos puntos de extensión (mapeo de DTO, validación de campos propios) que el código sería más difícil de seguir que la duplicación directa, y porque el patrón de consultas ya existente en el proyecto eligió duplicar antes que generalizar.
