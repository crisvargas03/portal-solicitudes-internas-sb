# ADR-0007: Modelo propio de `Usuario` con `PasswordHash`, sin ASP.NET Core Identity

**Estado:** Aceptada
**Fecha:** 2026-09-09

## Contexto

El modelo mínimo describe `Usuario` con Id, nombre, email, rol y activo, y anota que "se puede usar Identity o un modelo propio simplificado". Pero el endpoint `POST /api/auth/login` necesita credenciales verificables, y el modelo mínimo no menciona ningún campo para ellas.

Al mismo tiempo, `docs/architecture.md` establece que `Domain` no depende de ninguna otra capa: "sin dependencias externas (sin EF Core, sin ASP.NET)".

## Decisión

Se usa un modelo propio simplificado: `Usuario` hereda de `EntidadBase` y agrega `PasswordHash` a los campos del modelo mínimo. La entidad solo custodia el hash; el algoritmo de hasheo y verificación se implementa en `Infraestructure` detrás de una interfaz declarada en `Application`, y la emisión del JWT queda en esa misma capa.

## Consecuencias

- **A favor:** `Domain` se mantiene sin un solo paquete NuGet, que es la comprobación mecánica de que la regla de dependencias se respeta; el modelo de usuario contiene exactamente lo que el requerimiento pide, sin las once tablas que Identity arrastra (claims, tokens, logins externos, lockout) y que este alcance no usa; los tres roles se resuelven con la enumeración `RolUsuario` en lugar de una tabla de roles genérica.
- **En contra / trade-offs:** hay que implementar a mano el hasheo, la política de contraseñas y la verificación, con el riesgo que eso implica frente a una biblioteca probada; no se obtienen gratis funcionalidades como bloqueo por intentos fallidos, confirmación de correo o recuperación de contraseña, que habría que construir si el alcance creciera.
- **Alternativas descartadas:** `Usuario : IdentityUser<int>` con ASP.NET Core Identity. Se descartó porque obligaría a `Domain` a referenciar `Microsoft.AspNetCore.Identity` y, con ello, EF Core, rompiendo la regla de dependencias que la propia arquitectura declara como criterio.
