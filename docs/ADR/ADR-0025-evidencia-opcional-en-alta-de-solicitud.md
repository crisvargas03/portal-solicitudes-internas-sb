# ADR-0025: Evidencia opcional en el alta de solicitud, como POST encadenado no transaccional

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

[ADR-0006](ADR-0006-adjunto-como-entidad-propia.md) modela la evidencia como la entidad `Adjunto`,
hija de `Solicitud`, con su propio endpoint `POST /api/solicitudes/{id}/adjuntos` — que necesita un
`SolicitudId` ya existente. El formulario de alta de solicitud, sin embargo, quería ofrecer el
campo de evidencia junto con el resto de los datos en un solo paso, no solo desde el detalle
después de creada. Eso deja dos preguntas: cómo encajar un campo que depende de un id que todavía
no existe al momento de tipear el formulario, y qué validación de formato aplicarle a la URL,
dado que el validador del comando (`CrearAdjuntoCommandValidator`) no exige ningún formato,
solo longitud máxima y que no esté vacío.

## Decisión

El formulario de alta hace dos llamadas HTTP en secuencia, no una transacción: primero
`POST /api/solicitudes` y, si la evidencia fue completada, `POST /api/solicitudes/{id}/adjuntos`
con el id recién obtenido. Si el segundo POST falla, la solicitud ya creada no se revierte ni se
pierde — se navega igual a su detalle y se avisa con un toast de advertencia que la evidencia debe
agregarse desde ahí (ver [ADR-0024](ADR-0024-feedback-de-operaciones-con-toasts.md)).

La validación de la URL es deliberadamente laxa y **solo en el cliente**: opcional, pero si se
completa debe empezar con `http://` o `https://`. El backend no cambia — sigue sin exigir formato,
porque el requerimiento admite tanto una URL como "una referencia en texto", y una regex más
estricta en el servidor rechazaría referencias de texto plano válidas.

## Consecuencias

- **A favor:** el usuario captura la evidencia en el mismo formulario donde ya está describiendo
  la solicitud, sin una segunda visita a la pantalla de detalle en el caso común. No se necesita
  una transacción distribuida ni un endpoint nuevo — se reutiliza `Adjunto` tal como quedó
  decidido en ADR-0006. La validación de URL en el cliente atrapa el error más común (pegar una
  ruta de archivo en vez de un link) sin bloquear una referencia de texto legítima.
- **En contra / trade-offs:** existe una ventana entre ambos POST en la que la solicitud existe
  sin su evidencia — si el segundo falla, el usuario debe volver a intentarlo manualmente desde el
  detalle. La regla de URL es puramente de cliente: un `curl` directo contra la API puede guardar
  cualquier texto en `Url`, igual que antes de este cambio.
- **Alternativas descartadas:** bloquear la creación completa de la solicitud si el POST de
  adjunto falla (deshacer la solicitud ya creada), descartada porque el negocio prioriza que la
  solicitud quede registrada — la evidencia es explícitamente opcional en el requerimiento
  original, no debería poder tumbar la creación de un caso obligatorio. Exigir formato de URL con
  `z.url()` estricto, descartada porque el propio requerimiento admite una referencia de texto sin
  forma de URL en el mismo campo (ver ADR-0006).
