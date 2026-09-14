# ADR-0011: Generación del código legible de Solicitud

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

`docs/open-decisions.md` (entrada #2, retirada por este ADR) dejaba abierto cómo generar el código con formato `SOL-2026-0001`: `Solicitud.Codigo` solo validaba formato y longitud, sin generar nada. Las opciones sobre la mesa eran una `SEQUENCE` de SQL Server por año, una tabla contadora con bloqueo, o consultar el máximo actual del año (descartable de entrada por condiciones de carrera bajo creación concurrente).

## Decisión

Tabla `ContadorCodigo(Anio int PK, Ultimo int)`, una fila por año, poblada perezosamente (la primera solicitud del año la crea). `IGeneradorCodigoSolicitud` (Application) la actualiza mediante SQL crudo dentro de la transacción que crea la solicitud:

```sql
UPDATE ContadoresCodigo WITH (UPDLOCK, ROWLOCK)
SET @siguiente = Ultimo = Ultimo + 1
WHERE Anio = @anio;

IF @@ROWCOUNT = 0
BEGIN
    BEGIN TRY
        INSERT INTO ContadoresCodigo (Anio, Ultimo) VALUES (@anio, 1);
        SET @siguiente = 1;
    END TRY
    BEGIN CATCH
        UPDATE ContadoresCodigo WITH (UPDLOCK, ROWLOCK)
        SET @siguiente = Ultimo = Ultimo + 1
        WHERE Anio = @anio;
    END CATCH
END
```

`UPDLOCK, ROWLOCK` serializa el incremento cuando la fila del año ya existe: dos creaciones concurrentes no pueden leer el mismo `Ultimo`. Para el primer registro de un año nuevo la fila todavía no existe, así que el `UPDATE` inicial afecta cero filas y el código cae al `INSERT`; si dos transacciones concurrentes llegan a esa rama a la vez, la segunda choca contra la clave primaria y el `CATCH` la hace caer de vuelta al mismo `UPDATE` con bloqueo, que ahora sí encuentra la fila que acaba de crear la primera. El año nuevo es, en la práctica, una fila nueva: no hay coordinación especial para el rollover, ocurre solo. El índice único existente sobre `Solicitud.Codigo` queda como respaldo final ante cualquier caso no cubierto por este mecanismo.

## Consecuencias

- **A favor:** race-safe tanto en el caso estable (fila ya existe) como en el caso límite (primer registro del año), sin `SEQUENCE` propietaria de SQL Server ni lógica de reinicio anual explícita; el mecanismo vive en una sola clase (`GeneradorCodigoSolicitud`) fácil de auditar.
- **En contra / trade-offs:** requiere SQL crudo en lugar de LINQ, porque `SELECT ... WITH (UPDLOCK, ROWLOCK) ... SET x = y = y+1` con salida por parámetro no tiene una forma idiomática en EF Core; la tabla `ContadorCodigo` no participa del patrón de repositorio genérico (`IRepositorioBase<T>`) porque su única operación es este incremento atómico, no CRUD.
- **Alternativas descartadas:** `SEQUENCE` de SQL Server, descartada porque las secuencias nativas no se reinician por año sin lógica adicional y complican la migración inicial; consultar `MAX(Id) + 1` del año, descartada de entrada por el requerimiento mismo, al ser insegura bajo escritura concurrente.
