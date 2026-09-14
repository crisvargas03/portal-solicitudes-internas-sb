# ADR-0014: LiteBus como mediador de comandos y consultas, en lugar de MediatR

**Estado:** Aceptada
**Fecha:** 2026-09-10

## Contexto

`docs/architecture.md` nombraba MediatR como el mediador de comandos/consultas para los handlers de `Application/Features` (ver [handlers en lugar de casos de uso](../architecture.md)). MediatR a partir de su v12 pasó a licencia comercial, lo que lo descarta para un proyecto que busca dependencias de código abierto. Hacía falta un mediador CQS ligero, MIT, activamente mantenido, compatible con `net8.0` (la solución completa está fijada a .NET 8, ver `docs/CLAUDE.md`).

## Decisión

**LiteBus** (`litenova/LiteBus`, MIT), pinneado a la **versión 5.0.0 exacta** — es la última línea que publica un build específico para `net8.0`; 6.x y 7.x solo targetean `net10.0`. Paquetes: `LiteBus.Commands`, `LiteBus.Queries`, `LiteBus.Runtime.Extensions.Microsoft.DependencyInjection` (los tres, exactamente esos — ver la nota siguiente). Registro:

```csharp
services.AddLiteBus(bus =>
{
    bus.AddCommandModule(m => m.RegisterFromAssembly(assembly));
    bus.AddQueryModule(m => m.RegisterFromAssembly(assembly));
});
```

`Microsoft.Extensions.DependencyInjection.Abstractions` se fija en `10.0.8` en `Application` e `Infraestructure` (no en el `8.0.2` que traía el proyecto): LiteBus 5.0.0 exige ese piso incluso en su grupo de dependencias para `net8.0` — es una decisión de empaquetado de LiteBus, no una incompatibilidad real de framework. Verificado de punta a punta: build limpio, la API arranca, y Swagger responde 200 con esa versión de DI.Abstractions corriendo sobre un host `net8.0`.

**Hallazgos verificados empíricamente** (no leídos de la documentación pública, que en dos puntos resultó incorrecta o desactualizada respecto al paquete real):

1. **Los paquetes `LiteBus.Commands.Extensions.Microsoft.DependencyInjection` y `LiteBus.Queries.Extensions.Microsoft.DependencyInjection`, que el README oficial da como parte de la instalación, no contienen ningún tipo en la versión 5.0.0** (`Assembly.GetTypes().Length == 0`, confirmado por reflexión). El registro real (`AddCommandModule`/`AddQueryModule`) vive en `LiteBus.Commands`/`LiteBus.Queries`; `AddLiteBus` vive en `LiteBus.Runtime.Extensions.Microsoft.DependencyInjection`.
2. **Los handlers abiertos por tipo genérico admiten exactamente un parámetro genérico.** Un `ICommandPostHandler<TCommand, TResult>` (dos parámetros) como clase abierta lanza `UnsupportedOpenGenericHandlerException` al registrarse. Los comportamientos transversales de esta solución (`RegistroComandos<T>`, `RegistroConsultas<T>`, `ValidadorComandos<T>`, `ValidadorConsultas<T>`) usan las formas de un solo parámetro (`ICommandPostHandler<T>`/`ICommandErrorHandler<T>`, con el resultado como `object?` sin tipar) por esta razón.
3. **Registrar cualquier `ICommandErrorHandler<T>` (abierto o cerrado) suprime la excepción original por defecto** — `SendAsync` retorna normalmente en vez de propagarla, a menos que el manejador la relance explícitamente (`ExceptionDispatchInfo.Capture(exception).Throw();`). Confirmado con una repetición controlada: con el manejador de error registrado y sin relanzar, una validación que debía fallar no llegaba al llamador. `RegistroComandos<T>.HandleErrorAsync` releva después de loguear — sin ese relanzamiento, ningún error llega al middleware de excepciones de la Api y todo el contrato de ADR-0010 se rompe en silencio para cualquier comando.

**Eventos de LiteBus deliberadamente sin usar.** Las notificaciones (ADR-0003) pasan por `INotificationService` invocado directamente desde los handlers; enrutarlas como `IEvent` de LiteBus agregaría una capa sin beneficio a esta escala.

## Consecuencias

- **A favor:** licencia MIT sin ambigüedad; separación explícita `ICommandMediator`/`IQueryMediator` que hace visible en la firma del controlador si una acción es de lectura o escritura; los tres hallazgos de arriba, una vez conocidos, dan un pipeline de validación + logging tan capaz como el de MediatR con `IPipelineBehavior<,>`.
- **En contra / trade-offs:** fijar 5.0.0 acopla cualquier mejora futura de LiteBus a una migración previa a `net10.0`, porque 6.x rompe el build en `net8.0`; el mediador de comandos/consultas de LiteBus, a diferencia de `IPipelineBehavior<,>`, no permite envolver al handler (ningún `try/finally` alrededor de la llamada) — cada etapa (pre/post/error) es un punto de enganche separado, no una cadena continua; el paquete `LiteBus.Runtime.Extensions.Microsoft.DependencyInjection` no documenta su piso agresivo de `DependencyInjection.Abstractions`, así que cualquier futura actualización de patch de LiteBus dentro de la propia serie 5.x debe volver a verificarse contra `net8.0` en vez de asumir compatibilidad por el número de versión.
- **Alternativas descartadas:** MediatR, descartado por la licencia comercial desde v12; escribir un mediador propio minimalista, descartado por ser una reinvención innecesaria de algo que LiteBus ya resuelve, una vez entendidas sus particularidades.
