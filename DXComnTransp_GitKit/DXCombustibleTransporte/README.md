# DXComnTransp (COMBTRANSP)

Sistema de gestión de combustible y transporte construido con **DevExpress XAF 22.1.3**, **EF Core** y **.NET 6**, con dos clientes: escritorio **Windows Forms** y web **Blazor Server**, sobre **SQL Server**.

## Estructura de la solución

| Proyecto | Descripción |
|---|---|
| `DXComnTransp.Module` | Módulo XAF: business objects (EF Core), controllers, reportes, migraciones |
| `DXComnTransp.Win` | Aplicación de escritorio (Windows Forms) |
| `DXComnTransp.Blazor.Server` | Aplicación web (Blazor Server + JWT + Cookies) |

## Requisitos para compilar

- **Visual Studio 2022** 17.x con carga de trabajo "Desarrollo de escritorio de .NET" y "Desarrollo web y ASP.NET"
- **SDK de .NET 6**
- **DevExpress 22.1.3** instalado (los proyectos referencian DLLs desde `C:\Program Files\DevExpress 22.1\...` y paquetes NuGet del feed de DevExpress)
- **SQL Server** (Local) o **LocalDB**; por defecto: `(localdb)\mssqllocaldb`, base de datos `COMBTRANSP`

## Configuración (fuera del código fuente)

No hay credenciales en el repositorio. Configurar antes de ejecutar:

**Cadena de conexión** (variable de entorno o user-secrets):
```bash
# Variable de entorno
ConnectionStrings__ConnectionString="Data Source=...;Initial Catalog=COMBTRANSP;..."

# Desarrollo local (en la carpeta del proyecto Blazor o Win)
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<cadena>"
```

**Clave JWT** (Blazor, mínimo 32 caracteres):
```bash
dotnet user-secrets set "Authentication:Jwt:IssuerSigningKey" "<clave-larga>"
```

**Password inicial del usuario Admin** (opcional, mínimo 12 caracteres; lo crea el `Updater`):
```bash
DXCOMNTRANSP_INITIAL_ADMIN_PASSWORD="<password-inicial>"
```

## Notas técnicas importantes

- El consecutivo de las compras de combustible lo genera el **trigger SQL** `TR_ComprasCombustibleResumen_Consecutivo` (creado por la migración `BaselineBusinessSchema`).
- `ReporteVehiculo` está mapeado a la **vista SQL** `dbo.vw_ReporteVehiculos` (no es tabla).
- `CompraCombustibleDetalle.Precio` es una **columna computada** en SQL Server.
- Los archivos `Model.DesignedDiffs.xafml`, `Model.xafml` y `UnusableNodes.xml` **se versionan a propósito**: contienen personalización del modelo XAF. No eliminarlos del repositorio.
- Las migraciones de EF Core están en `DXComnTransp.Module/Migrations` y también se versionan.

## Flujo de trabajo con Git

- Rama principal: `main`
- Cada funcionalidad nueva: rama `feature/nombre-corto`, PR a `main`
- No commitear nunca: `bin/`, `obj/`, `.vs/`, secretos (ver `.gitignore`)
