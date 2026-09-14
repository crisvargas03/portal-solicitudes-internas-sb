# ADR-0013: Validación de campos con FluentValidation y su relación con Resultado

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

El requerimiento pide "validación de campos obligatorios y longitud máxima de texto" (ver `docs/open-decisions.md` #3, resuelta aparte para los valores concretos). [ADR-0010](ADR-0010-resultado-y-contrato-de-respuesta.md) ya distingue fallos de negocio esperados (`Resultado.Fallido`) de lo genuinamente excepcional. Faltaba decidir dónde vive la validación de forma de un comando/consulta — campos vacíos, longitudes, formato de email — y cómo se conecta con ese contrato de respuesta sin duplicar la distinción de ADR-0010.

## Decisión

**FluentValidation**, un `AbstractValidator<T>` por comando/consulta en la misma carpeta de features (`Application/Features/<Area>/Commands/<Nombre>/<Nombre>Validator.cs`), leyendo las constantes `MAX_LONGITUD_*` que ya declaran las entidades de `Domain` — nunca literales repetidos, para que la longitud de columna (EF Core), el validador y cualquier otra referencia queden atadas a una sola fuente.

**Ejecución vía LiteBus, no como parte de ADR-0010.** Dos clases abiertas de un solo parámetro genérico (`ValidadorComandos<T>` / `ValidadorConsultas<T>`, ver ADR-0014 sobre esa restricción de LiteBus) implementan `ICommandPreHandler<T>` / `IQueryPreHandler<T>`, resuelven los validadores registrados para `T` y lanzan `FluentValidation.ValidationException` si hay fallos. Esa excepción la traduce `ManejadorExcepcionValidacion` (un `IExceptionHandler` de .NET 8) a `ValidationProblemDetails` con el diccionario de errores por campo — 400, con el mismo formato `ProblemDetails` que ADR-0010 ya usa para los fallos de `Resultado`.

**Por qué excepción y no `Resultado` para esto.** La validación de forma ocurre *antes* de que exista una instancia del handler — es un pre-handler que corre alrededor del pipeline de LiteBus, no dentro de un `HandleAsync` que pueda construir y devolver un `Resultado<T>`. Usar una excepción para esta capa, y `Resultado` para las reglas de negocio dentro del handler, mantiene la distinción de ADR-0010 intacta: `Resultado` es para lo que el handler decide con datos de dominio (transición no permitida, correo duplicado); la excepción de validación es para lo que se puede rechazar sin tocar la base de datos.

## Consecuencias

- **A favor:** un validador por comando es fácil de ubicar y de testear aislado; las longitudes quedan en una sola fuente (`Domain`); el cliente recibe el mismo `errors: { campo: [mensajes] }` para cualquier comando, sin que cada handler decida su propio formato.
- **En contra / trade-offs:** la validación de forma vive fuera del handler, así que un handler no puede asumir "si llegué aquí, el comando es válido" sin acordarse de que el pre-handler ya lo filtró — es implícito, no un tipo que lo garantice en compilación.
- **Alternativas descartadas:** validar dentro de cada `HandleAsync` con `if` y devolver `Resultado.Fallido(Error.Validacion(...))`, descartada por repetir la misma lógica de "campo vacío / longitud" en cada handler en lugar de declararla una vez por tipo de comando.
