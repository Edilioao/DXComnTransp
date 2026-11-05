# AGENTS.md — DXComnTransp

ERP de gestión de transporte y combustible (SGCOMB-TRANS) sobre **DevExpress XAF 22.1.3 + EF Core**, target `net6.0`. UI Blazor Server (principal) y WinForms. Todo el dominio está en español (es-MX).

## Comandos

```powershell
# Verificación principal (no hay suite de tests; EasyTest existe solo como configuración)
dotnet build DXComnTransp.sln -c Debug

# Compilar un solo proyecto (más rápido para iterar)
dotnet build DXComnTransp.Blazor.Server\DXComnTransp.Blazor.Server.csproj -c Debug
```

- El SDK instalado es **.NET 8/9**, pero los proyectos apuntan a `net6.0`: compila por el targeting pack 6.0. No "modernizar" el target.
- El usuario desarrolla/ejecuta desde **Visual Studio (IIS Express, puertos 65201/44318)**. NO lanzar `dotnet run` ni IIS Express manual: bloquea las DLL de `bin` y rompe el build del usuario. Verificar con un "touch" de apertura en modo `ReadWrite` que `DXComnTransp.Blazor.Server.dll` esté libre antes de compilar.
- Update de BD por CLI: `DXComnTransp.Blazor.Server --updateDatabase [--forceUpdate --silent]`.

## Estructura

| Proyecto | Rol |
|---|---|
| `DXComnTransp.Module` | **Dominio compartido**: BusinessObjects EF Core, migraciones, reportes XtraReport, controllers genéricos, `Model.DesignedDiffs.xafml` (personalización de vistas con Model Editor) |
| `DXComnTransp.Blazor.Server` | App Blazor: controllers específicos Blazor, Startup, visor de reportes PDF |
| `DXComnTransp.Win` | App WinForms: controllers específicos Win |

- Los controllers que deben funcionar en las dos UIs van en `Module/Controllers`; para comportamiento diferente por plataforma se duplica en `Blazor.Server/Controllers` y `Win/Controllers` (ej.: `CompraCombustibleImprimirController`). Un cambio de comportamiento en uno puede requerir espejo en el otro.
- `Controllers/CompraCombustibleResumenController.cs` está excluido del build vía `<Compile Remove>` en el csproj: no está compilado a propósito, no intentar "arreglar" referencias de él ni eliminar la exclusión sin confirmar.
- `DXComnTransp_GitKit\` es un árbol duplicado de referencia, NO parte del build.

## Configuración y datos

- Cadena de conexión y claves **nunca** en el repo: `ConnectionStrings__ConnectionString` (o `DXCOMNTRANSP_CONNECTIONSTRING`), JWT: `Authentication:Jwt:IssuerSigningKey`, password admin inicial: `DXCOMNTRANSP_INITIAL_ADMIN_PASSWORD` (mín. 12 chars). En dev: `dotnet user-secrets` (UserSecretsId `336978a4-...` en el csproj).
- Fallback de conexión para design-time: `(localdb)\mssqllocaldb;Initial Catalog=COMBTRANSP` (en `DXComnTranspDbContext.cs`).
- Migraciones EF Core en `DXComnTransp.Module\Migrations`. La migración `20261102214046_AgregaNoContenedorADetalle` no está aplicada a la BD aún (decisión del usuario: no tocar la BD por ahora).
- Semillas de datos/usuarios en `DXComnTransp.Module\DatabaseUpdate\Updater.cs` (roles, admin, tipos de documento).
- La cultura es `es-MX` y se fija en el `Program.cs` de **cada** app (Blazor y Win); replicar ahí cualquier ajuste cultural.

## Reportes PDF de compras (flujo delicado, no romper)

1. `CompraCombustibleImprimirController.cs` (Blazor) genera el XtraReport con `CompraCombustibleReport.Crear()` (ad-hoc, DTO planos, sin DataSource EF) → exporta PDF → lo guarda en `ReporteCompraPdfCache` (memoria, 15 min, máx 10).
2. Sirve el PDF `/reportes/compra-combustible-pdf/{token}` (`ReporteCompraPdfEndpointController`, requiere cookie de sesión, inline).
3. El visor se muestra con **DxPopup** de DevExpress (componente `Components/VisorReporte.razor`, montado como root component en `Pages/_Host.cshtml`), coordinado por `VisorReporteService` (Scoped por circuito).

Lecciones ganadas a fuerza de depurar (ver `%TEMP%\dx-reportes.log`, escrito por `DiagnosticoReportes`):
- **NO usar JS interop dentro del handler de un SimpleAction ni `window.open`**: deja el circuito en mal estado y los botones de acción de XAF dejan de ejecutar (primera vez sí, después nada, aunque los clics en filas funcionen). El controller usa solo el servicio Scoped (no interop); la impresión a pestaña nueva es un `<a target="_blank">` real dentro del visor.
- `TargetViewNesting = Nesting.Any` es necesario para que la acción exista también en DetailView anidados (con `Nesting.Root` el botón no aparece en el detalle).
- **Suscribir `action.Execute` en `OnActivated` (con `-=; +=`) y desuscribir SOLO en `OnDeactivated`**: XAF reutiliza la misma instancia del controller entre list⇄detalle; si se suscribe solo en el constructor y se desengancha en `OnDeactivated`, tras la primera desactivación la acción "no hace nada" (el clic llega al servidor sin manejador) hasta recargar la página entera.
- Los overlays/visores propios con CSS deben eliminarse al cerrar; un overlay remanente bloquea todos los clics de la app al volver a la lista.

## Entradas de combustible e inventario (mapeo de tablas legadas)

- `EntradaCombustible`/`EntradaCombustibleDetalle`/`Inventario` se mapean a las tablas legadas `RecepcionResumen`/`RecepcionDetalle`/`Inventarios` con `[Table(...)]`. **No hay migración EF para ellas** (las tablas ya existen; no tocar la BD). Los tipos Id escalares deben respetar la DDL legada exacta (Inventarios usa `bigint` → `long`).
- La BD legada tiene triggers en turnos `TR_RecepcionResumen_*`/`TR_RecepcionDetalle_*`: el código no debe duplicar lo que hacen (terminada/bloqueos), solo convivir (terminar/cancelar actualiza `Inventarios` en el MISMO ObjectSpace y un solo `CommitChanges`).
- Inventario: precio = **promedio ponderado** (Precio×TasaCambio de línea); Cancelar revierte con fórmula inversa ("desponderar") con guardas de >0 en precio y existencia.
- Precio del detalle de entrada **no es editable** (`[ModelDefault("AllowEdit","False")]`): el controller lo sugiere de `Inventario` del depósito (si existe) o del `PCosto` de `TipoCombustible`.
- Consecutivo de entradas se genera en código (`IXafEntityObject.OnSaving` + `IObjectSpaceLink`); el trigger de consecutivo solo existe en compras (encriptado e inaccesible). Implementar `IObjectSpaceLink` con **implementación explícita de interfaz** para que la propiedad no aparezca en vistas (no `[NotMapped]`, que no basta allí).

## Gotchas de DevExpress 22.1.3 EF Core (atributos)

- **`[DefaultProperty]` NO está en `DevExpress.Persistent.Base`** (CS0246/CS0234 lo confirma). En este setup se resuelve con `using System.ComponentModel;`. `DefaultClassOptions`, `NavigationItem`, `VisibleInListView/DetailView/LookupListView`, `RuleRequiredField` etc. sí están en `DevExpress.Persistent.Base`/`DevExpress.Persistent.Validation`.
- `SelectionDependencyType` solo tiene `RequireMultipleObjects` y `RequireSingleObject` (no hay "NoSelection"/"IndependentOf"): para una acción independiente de la selección, **omitir la propiedad**.
- `SimpleAction`/controller compartidos Blazor+Win: usar el patrón `OnActivated` (`-=;+=`) descrito arriba (aplicado también a `CompraCombustibleController`).

## Model Editor / vistas

- La personalización de vistas (orden de columnas, índices) vive en `DXComnTransp.Module\Model.DesignedDiffs.xafml`; `Model.xafml` de cada app es el nodo de aplicación.
- XAF guarda diferencias de modelo por usuario/Admin en la BD: si una vista "no respeta" el `.xafml`, puede haber un `ModelDifferenceAspects` usuario que la domine (hubo que limpiar el de `Admin` a mano).

# Gotchas de Windows/edición

- El repositorio tiene tildes en código y comentarios. Al editar desde PowerShell **no usar `Set-Content` sin `-Encoding UTF8`**: corrompe acentos a mojibake (`Acción`→`AcciA3n`). Preferir herramientas de edición de archivos; validar con `ReadAllText(..., Encoding.UTF8)` buscando U+FFFD.
- La app usa SignalR/puertos específicos de IIS Express; los datos de `launchSettings.json` concuerdan con las URLs que comparte el usuario (`https://localhost:44318/...`).
