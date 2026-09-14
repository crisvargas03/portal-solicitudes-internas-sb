# ADR-0036: Formularios admin sin `reset()` en un efecto

**Estado:** Aceptada
**Fecha:** 2026-09-14

## Contexto

Al construir la pantalla de administración de Entidades gubernamentales (ver ADR-0034), su formulario de alta/edición mostraba "Campo obligatorio" en los 4 campos aunque el usuario ya los había llenado (capturado en pantalla por el usuario). El comportamiento era extraño: los `<input>` sí tenían el texto escrito, pero al enviar el formulario `react-hook-form` (con `zodResolver`) reportaba todos los campos como vacíos.

**Diagnóstico.** El primer intento de reproducirlo con `vitest` + `jsdom` fue engañoso: en ese entorno, hasta un `reset()` de `react-hook-form` sin ningún `resolver` de por medio fallaba en actualizar el valor del `<input>`, algo que no ocurre en un navegador real — jsdom no es un oráculo confiable para este caso puntual. La reproducción válida se hizo con Chrome headless real (vía Puppeteer, conectado a una instancia de Chrome ya instalada en la máquina), automatizando: login → ir al formulario → escribir los 4 campos → enviar → leer los errores visibles y el valor de los inputs. Con eso quedó confirmado en un navegador real:

- El valor de los `<input>` era correcto antes **y después** de enviar (p. ej. `"Prueba"`, `"Banco"`, `"Poder Ejecutivo"`, `"Presidencia"`).
- Aun así, los 4 campos quedaban marcados como inválidos con el mensaje de "obligatorio" del propio schema de Zod.

La causa era el patrón usado para poblar el formulario al editar un registro existente:

```tsx
const { register, handleSubmit, reset, formState } = useForm({
  resolver: zodResolver(schema),
  mode: 'onTouched',
  defaultValues: { nombre: '', /* ... */ },
});

useEffect(() => {
  reset({ nombre: editando?.nombre ?? '', /* ... */ });
}, [editando, reset]);
```

Ese `useEffect` se ejecuta también en el montaje inicial (con `editando` en `null`), llamando a `reset()` con los mismos valores vacíos que ya traía `defaultValues`. Con la combinación exacta de versiones del proyecto (`react-hook-form` 7.88, `@hookform/resolvers` 5.9, `zod` 4.6, React 19.2), esa llamada a `reset()` — aunque no cambie ningún valor — deja el registro interno del campo en un estado donde `handleSubmit` vuelve a leer el `defaultValue` original en vez del valor tipeado, y el resolver de Zod lo reporta como vacío. Se confirmó con un caso mínimo (un único campo `nombre`, sin relación con el schema de Entidades gubernamentales) que el mismo patrón —`reset()` disparado desde un efecto tras el montaje— reproduce el bug igual.

**Alcance real del bug.** El mismo patrón (`useEffect` + `reset()` para sincronizar el formulario con el ítem que se edita) se usaba, además de en el formulario nuevo, en:

- `AreasAdminSection.tsx`
- `PrioridadesAdminSection.tsx`
- `TiposSolicitudAdminSection.tsx`
- `AdminUsuarios.tsx` (formulario de edición; el de alta también llamaba `reset()`, pero desde el manejador de envío exitoso, no desde un efecto)

Es decir: **crear una nueva Área, Prioridad, Tipo de solicitud o Entidad gubernamental estaba roto en producción** en el momento de este hallazgo — el usuario podía escribir los datos, pero nunca lograba enviarlos.

## Decisión

Eliminar el patrón "un `useEffect` que llama `reset()` para sincronizar el formulario con una prop" en los cinco formularios administrativos. En su lugar, cada formulario se extrae a su propio subcomponente, montado con una `key` de React que cambia cuando debe arrancar de cero:

- **Editar un registro existente:** `key={editando.id}` (o el subcomponente solo se monta cuando `editando` no es `null`, como en `AdminUsuarios`). Cambiar de "editar el registro A" a "editar el registro B" fuerza un remount limpio con los valores de B en `defaultValues` — sin `reset()`.
- **Crear uno nuevo:** `key={`nuevo-${formularioVersion}`}`, con `formularioVersion` un contador que se incrementa tras un alta exitosa. Eso fuerza un remount con el formulario vacío para el siguiente registro, en vez de llamar `reset()` sobre el formulario ya montado.

`defaultValues` se calcula directamente a partir de la prop (`editando?.nombre ?? ''`, etc.) en el momento de montar — nunca se vuelve a asignar después. Ningún formulario admin llama `reset()`.

Archivos tocados: `AreasAdminSection.tsx`, `PrioridadesAdminSection.tsx`, `TiposSolicitudAdminSection.tsx`, `EntidadesGubernamentalesAdminSection.tsx`, `AdminUsuarios.tsx`.

**Verificación:** los cinco flujos (crear en los cuatro catálogos, más crear y editar usuario) se probaron de punta a punta contra la app real corriendo en Docker + Chrome headless real: sin errores, valores correctos, formulario limpio tras crear.

## Consecuencias

- **A favor:** elimina la clase de bug completa, en vez de parchear el síntoma (por ejemplo, agregando un `setTimeout` antes del `reset()`, que hubiera sido fresco pero frágil y dependiente de timing). Es además el patrón que la propia documentación de React recomienda para "reiniciar o ajustar el estado cuando cambia una prop": usar `key` para remontar, no un efecto que empuje el nuevo valor.
- **En contra / trade-offs:** cada sección admin gana un subcomponente adicional (el formulario), por lo que hay un archivo más grande dividido en dos funciones en vez de una. `AdminUsuarios.tsx` en particular pasó de tener un solo `useForm` reutilizado a dos subcomponentes (`UsuarioCrearForm`, `UsuarioEditarForm`), ligeramente más código pero cada uno con su propio ciclo de vida claro.
- **Alcance no cubierto:** no se investigó si `reset()` fuera de este patrón específico (por ejemplo, uno que sí cambia valores reales, no un no-op de montaje) también dispara el bug — se optó por eliminar el patrón entero en los formularios admin en vez de acotar el arreglo al caso mínimo reproducido.
- **Alternativas descartadas:** version-pinning distinto de `react-hook-form`/`zod`/`@hookform/resolvers` para esquivar el bug — descartado por no tener certeza de qué combinación de versiones lo introduce, y porque el arreglo por `key` es correcto independientemente de la causa exacta en la librería.
