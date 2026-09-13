# ADR-0029: El alcance por rol falla cerrado

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

`AlcanceSolicitudesFactory.Calcular` tenía un caso `_ => AlcanceSolicitudes.SinRestriccion()` como
default. Dos caminos llegaban ahí sin que el usuario fuera Administrador: un claim de rol
(`ClaimTypes.Role`) que no parsea a ningún valor de `RolUsuario` (`IUsuarioActual.Rol` es
`RolUsuario?`), y un Solicitante o Analista cuyo claim `NameIdentifier` no parsea a `int`
(`IUsuarioActual.Id` también nulable) — en ese segundo caso, `new AlcanceSolicitudes(null, null,
false)` es estructuralmente idéntico a `SinRestriccion()`: ve todo. Ambos requieren de todas formas
un JWT válido y firmado por el servidor (`[Authorize]` en ambos controladores), así que no son
explotables por un atacante externo sin credenciales — pero un default que amplía la visibilidad
ante un caso no contemplado es la forma equivocada de fallar: cualquier bug futuro en la emisión o
lectura del claim se traduciría silenciosamente en acceso total, en vez de en un error visible.

## Decisión

`AlcanceSolicitudesFactory.Calcular` devuelve `Resultado<AlcanceSolicitudes>` en vez de
`AlcanceSolicitudes`. El único camino a `SinRestriccion()` es `RolUsuario.Administrador`
explícito; Solicitante y Analista exigen además `usuarioActual.Id is int id` (pattern matching
sobre el nullable); cualquier otro caso —rol nulo, rol que no matchea ningún patrón, id ausente—
devuelve `Resultado.Fallido` con `Error.NoAutorizado("Auth.AlcanceIndeterminado", ...)`. Los dos
handlers que lo consumen (`ObtenerSolicitudesPaginadoQueryHandler`,
`ObtenerResumenDashboardQueryHandler`) hacen early-return del error, que `AResultadoHttp()` traduce
a 401.

## Consecuencias

- **A favor:** un JWT con un claim de rol o de id corrupto o inesperado nunca resulta en ver todas
  las solicitudes de la organización — resulta en un 401 explícito. El comportamiento por defecto
  ante un caso no contemplado es negar, no conceder.
- **En contra / trade-offs:** un cliente legítimo con un token mal formado ahora ve 401 en vez de
  (incorrectamente) una lista completa — es el comportamiento correcto, pero es un cambio observable
  si algo en el pipeline de emisión de tokens llegara a producir claims inconsistentes sin que nadie
  lo hubiera notado antes.
- **Alternativas descartadas:** lanzar una excepción en vez de `Resultado.Fallido`, descartada
  porque el resto de la capa de aplicación usa el patrón Resultado para todo fallo esperado (ver
  [ADR-0010](ADR-0010-resultado-y-contrato-de-respuesta.md)) — una excepción aquí rompería esa
  convención sin necesidad, ya que este es exactamente el tipo de fallo que el patrón existe para
  modelar como dato.
