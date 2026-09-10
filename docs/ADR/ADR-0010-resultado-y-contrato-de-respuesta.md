# ADR-0010: Patron Resultado y contrato estandar de respuesta de la API

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

Los handlers de MediatR que siguen a [ADR-0009](ADR-0009-repositorios-y-unidad-de-trabajo.md) necesitan una forma de comunicar fallos esperados de negocio — solicitud inexistente, transicion de estado no permitida (ADR-0005), correo duplicado — sin recurrir a excepciones para lo que no es excepcional. Ademas, el requerimiento pide que el frontend consuma una API consistente: sin un contrato unico, cada controlador termina devolviendo una forma de respuesta distinta segun quien lo escribio.

Quedaba por decidir donde empieza el patron Resultado (¿en el repositorio, en el handler?) y que forma toma la respuesta HTTP, tanto en exito como en fallo.

## Decision

**El patron Resultado empieza en el handler, no en el repositorio.** Los repositorios (ADR-0009) siguen devolviendo entidades planas y `null` para "no encontrado" — la ausencia es un hecho de persistencia, no un veredicto de negocio. El handler es quien decide que un `null` significa `Error.NoEncontrado(...)` y lo envuelve en `Resultado<T>`.

`Resultado` / `Resultado<T>` viven en `Application/Common/Resultados`. `Error` es un `record` con `Codigo` (clave estable con puntos, p. ej. `Solicitud.NoEncontrada`), `Descripcion` y `TipoError` (`Validacion`, `NoEncontrado`, `Conflicto`, `NoAutorizado`, `Prohibido`, `Falla`). `TipoError` es una categoria de negocio, no un codigo HTTP: `Application` no conoce HTTP.

La API estandariza toda respuesta:

- **Exito:** sobre `RespuestaApi<T>` (`{ exito, datos, mensaje }`). Los listados paginados colocan su `ResultadoPaginado<T>` en `datos`, sin una forma distinta para colecciones.
- **Fallo:** `ProblemDetails` (RFC 7807), no el sobre de exito. `ResultadoExtensions.AResultadoHttp()` mapea cada `TipoError` a un estado HTTP (`Validacion` → 400, `NoAutorizado` → 401, `Prohibido` → 403, `NoEncontrado` → 404, `Conflicto` → 409, `Falla` → 500) y arma el `ProblemDetails`, para que un controlador se reduzca a `return resultado.AResultadoHttp();`.

`Program.cs` agrega `AddProblemDetails()` para que tambien las excepciones no controladas devuelvan la misma forma.

## Consecuencias

- **A favor:** los handlers modelan los fallos esperados como datos, con pila de llamado clara y sin el costo de una excepcion; el codigo de error (`Codigo`) le da al frontend un valor estable para branchear sin parsear texto; un solo mapeo (`AResultadoHttp`) concentra toda la logica de traduccion a HTTP, evitando que cada controlador decida su propio estado; usar `ProblemDetails` para errores aprovecha soporte ya existente en ASP.NET Core y en Swagger.
- **En contra / trade-offs:** exito y fallo tienen formas de respuesta distintas (`RespuestaApi<T>` contra `ProblemDetails`) — se acepta porque el estado HTTP ya le dice al cliente que parser usar, y unificarlas forzaria un sobre de error mas pobre que `ProblemDetails`; `Resultado<T>.Valor` lanza si se lee en un resultado fallido, así que un handler mal escrito puede introducir un `InvalidOperationException` en tiempo de ejecucion en lugar de un error de compilacion.
- **Alternativas descartadas:** repositorios devolviendo `Resultado<T>` en toda lectura, descartada porque obliga al repositorio a nombrar errores de negocio (`Solicitud.NoEncontrada`), que es exactamente la logica que se quiso dejar en los handlers; un unico sobre para exito y error (`{ exito, datos, mensaje, errores[] }` con estado 200 siempre), descartada por resignar el soporte nativo de `ProblemDetails` y por ser menos idiomatica para quien consuma la API desde .NET u otra herramienta que ya entiende RFC 7807.
