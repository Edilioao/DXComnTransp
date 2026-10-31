-- ============================================================
-- Usuario de aplicación para DXComnTransp
--
-- Crea un login/usuario de SQL Server con permisos mínimos de
-- lectura y escritura. NO se le concede db_owner ni permisos DDL:
-- las actualizaciones de esquema deben ejecutarse con una cuenta
-- de despliegue distinta.
--
-- ANTES DE EJECUTAR:
--   1. Reemplace el marcador de la contraseña por una contraseña
--      robusta (mínimo 12 caracteres, mayúsculas, minúsculas,
--      dígitos y símbolos).
--   2. No guarde esta contraseña en el repositorio ni en
--      appsettings.json. Configúrela como variable de entorno
--      ConnectionStrings__ConnectionString en el servidor.
-- ============================================================
USE [COMBTRANSP];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'DXComnTranspApp')
BEGIN
    CREATE LOGIN [DXComnTranspApp]
        WITH PASSWORD = N'CAMBIO_OBLIGATORIO_AntesDeUsar_2026!',
             CHECK_POLICY = ON,
             CHECK_EXPIRATION = OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'DXComnTranspApp')
BEGIN
    CREATE USER [DXComnTranspApp] FOR LOGIN [DXComnTranspApp];
END
GO

IF IS_ROLEMEMBER(N'db_datareader', N'DXComnTranspApp') = 0
    ALTER ROLE db_datareader ADD MEMBER [DXComnTranspApp];
GO

IF IS_ROLEMEMBER(N'db_datawriter', N'DXComnTranspApp') = 0
    ALTER ROLE db_datawriter ADD MEMBER [DXComnTranspApp];
GO
