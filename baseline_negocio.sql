BEGIN TRANSACTION;
GO


IF OBJECT_ID(N'dbo.TipoCombustible', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoCombustible (
        Id int NOT NULL,
        Combustible nvarchar(60) NOT NULL,
        PCosto decimal(18,2) NOT NULL,
        PVenta decimal(18,2) NOT NULL,
        CONSTRAINT PK_TipoCombustible PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Depositos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Depositos (
        Id int NOT NULL,
        Deposito nvarchar(100) NOT NULL,
        Interno bit NOT NULL,
        Cerrado bit NOT NULL,
        CONSTRAINT PK_Depositos PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.TipoInventario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoInventario (
        Id int NOT NULL,
        TpoInventario nvarchar(100) NOT NULL,
        CONSTRAINT PK_TipoInventario PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Monedas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Monedas (
        Id int NOT NULL IDENTITY,
        Moneda nvarchar(6) NOT NULL,
        TasaCambio decimal(18,4) NOT NULL,
        CONSTRAINT PK_Monedas PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Proveedores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Proveedores (
        Id int NOT NULL IDENTITY,
        Proveedor nvarchar(500) NOT NULL,
        Activo bit NOT NULL,
        CONSTRAINT PK_Proveedores PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.UnidadMedida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UnidadMedida (
        Id int NOT NULL IDENTITY,
        UM nvarchar(8) NOT NULL,
        CONSTRAINT PK_UnidadMedida PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.TipoServicios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoServicios (
        Id int NOT NULL IDENTITY,
        Operacion nvarchar(100) NOT NULL,
        CONSTRAINT PK_TipoServicios PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Marcas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Marcas (
        Id int NOT NULL IDENTITY,
        Marca nvarchar(100) NOT NULL,
        CONSTRAINT PK_Marcas PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Modelos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Modelos (
        Id int NOT NULL IDENTITY,
        Modelos nvarchar(100) NOT NULL,
        CONSTRAINT PK_Modelos PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.TipoMovimientoCombustible', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoMovimientoCombustible (
        Id int NOT NULL IDENTITY,
        Codigo varchar(30) NOT NULL,
        Descripcion varchar(100) NOT NULL,
        EsEntrada bit NOT NULL,
        EsSalida bit NOT NULL,
        AfectaDisponibilidad bit NOT NULL,
        Activo bit NOT NULL,
        CONSTRAINT PK_TipoMovimientoCombustible PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Choferes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Choferes (
        Id bigint NOT NULL IDENTITY,
        CIdentidad nvarchar(30) NOT NULL,
        NombreChofer nvarchar(100) NOT NULL,
        Licencias nvarchar(100) NOT NULL,
        Telefono nvarchar(100) NULL,
        Activo bit NOT NULL,
        CONSTRAINT PK_Choferes PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.TipoDocumentoVehiculo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoDocumentoVehiculo (
        Id int NOT NULL IDENTITY,
        Nombre nvarchar(100) NOT NULL,
        RequiereVencimiento bit NOT NULL,
        Activo bit NOT NULL,
        CONSTRAINT PK_TipoDocumentoVehiculo PRIMARY KEY (Id)
    );
END;

IF OBJECT_ID(N'dbo.Configuracion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Configuracion (
        Id bigint NOT NULL IDENTITY,
        nombre varchar(200) NOT NULL,
        slogan varchar(250) NULL,
        logo image NULL,
        nit varchar(50) NOT NULL,
        reeup varchar(50) NULL,
        cuenta_bancaria varchar(100) NULL,
        correo varchar(160) NULL,
        telefonos text NULL,
        tasa_vigente decimal(12,4) NOT NULL,
        PathSalvaBD nvarchar(250) NULL,
        CONSTRAINT PK_Configuracion PRIMARY KEY (Id)
    );
END;

GO


IF OBJECT_ID(N'dbo.TipoVehiculos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoVehiculos (
        Id int NOT NULL IDENTITY,
        TipoVehiculo nvarchar(100) NOT NULL,
        IdMarca int NOT NULL,
        IdModelo int NOT NULL,
        IndiceNormado decimal(18,2) NOT NULL,
        Bateria nvarchar(100) NOT NULL,
        Neumaticos nvarchar(100) NOT NULL,
        CapTanqueComb decimal(18,2) NOT NULL,
        NoPlazas int NOT NULL,
        CapCarga decimal(18,2) NOT NULL,
        IdCombustible int NOT NULL,
        CONSTRAINT PK_TipoVehiculos PRIMARY KEY (Id),
        CONSTRAINT FK_TipoVehiculos_Marcas_IdMarca FOREIGN KEY (IdMarca) REFERENCES dbo.Marcas (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TipoVehiculos_Modelos_IdModelo FOREIGN KEY (IdModelo) REFERENCES dbo.Modelos (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TipoVehiculos_TipoCombustible_IdCombustible FOREIGN KEY (IdCombustible) REFERENCES dbo.TipoCombustible (Id) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.Vehiculos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehiculos (
        Id bigint NOT NULL IDENTITY,
        Matricula nvarchar(20) NOT NULL,
        IdTipoVehiculo int NOT NULL,
        IndiceReal decimal(18,2) NOT NULL,
        CapTanqueComb decimal(18,2) NOT NULL,
        IdCombustible int NOT NULL,
        IdTipoServicio int NOT NULL,
        Indicativo nvarchar(100) NULL,
        NoCarroceria nvarchar(100) NOT NULL,
        NoMotor nvarchar(100) NOT NULL,
        IdDeposito int NOT NULL,
        CONSTRAINT PK_Vehiculos PRIMARY KEY (Id),
        CONSTRAINT FK_Vehiculos_Depositos_IdDeposito FOREIGN KEY (IdDeposito) REFERENCES dbo.Depositos (Id) ON DELETE NO ACTION,
        -- NO ACTION (no CASCADE) para evitar el error 1785 de SQL Server:
        -- Vehiculos -> TipoVehiculos -> TipoCombustible y Vehiculos -> TipoCombustible
        -- forman rutas de cascada multiples sobre la misma tabla.
        CONSTRAINT FK_Vehiculos_TipoCombustible_IdCombustible FOREIGN KEY (IdCombustible) REFERENCES dbo.TipoCombustible (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Vehiculos_TipoServicios_IdTipoServicio FOREIGN KEY (IdTipoServicio) REFERENCES dbo.TipoServicios (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Vehiculos_TipoVehiculos_IdTipoVehiculo FOREIGN KEY (IdTipoVehiculo) REFERENCES dbo.TipoVehiculos (Id) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.DocumentosVehiculo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentosVehiculo (
        Id bigint NOT NULL IDENTITY,
        IdVehiculo bigint NOT NULL,
        IdTipoDocumento int NOT NULL,
        Descripcion nvarchar(200) NULL,
        FechaEmision datetime2 NULL,
        FechaVencimiento datetime2 NULL,
        ArchivoID int NULL,
        Observaciones nvarchar(500) NULL,
        CONSTRAINT PK_DocumentosVehiculo PRIMARY KEY (Id),
        CONSTRAINT FK_DocumentosVehiculo_FileData_ArchivoID FOREIGN KEY (ArchivoID) REFERENCES dbo.FileData (ID) ON DELETE NO ACTION,
        CONSTRAINT FK_DocumentosVehiculo_TipoDocumentoVehiculo_IdTipoDocumento FOREIGN KEY (IdTipoDocumento) REFERENCES dbo.TipoDocumentoVehiculo (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_DocumentosVehiculo_Vehiculos_IdVehiculo FOREIGN KEY (IdVehiculo) REFERENCES dbo.Vehiculos (Id) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'dbo.VehiculosChofer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehiculosChofer (
        Id bigint NOT NULL IDENTITY,
        IdVehiculo bigint NOT NULL,
        IdChofer bigint NOT NULL,
        IdChoferSdo bigint NULL,
        IdChoferTro bigint NULL,
        SysStartTime datetime2 NOT NULL CONSTRAINT DF_VehiculosChofer_SysStartTime DEFAULT (SYSUTCDATETIME()),
        SysEndTime datetime2 NOT NULL CONSTRAINT DF_VehiculosChofer_SysEndTime DEFAULT ('9999-12-31T23:59:59.9999999'),
        CONSTRAINT PK_VehiculosChofer PRIMARY KEY (Id),
        CONSTRAINT FK_VehiculosChofer_Choferes_IdChofer FOREIGN KEY (IdChofer) REFERENCES dbo.Choferes (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_VehiculosChofer_Choferes_IdChoferSdo FOREIGN KEY (IdChoferSdo) REFERENCES dbo.Choferes (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_VehiculosChofer_Choferes_IdChoferTro FOREIGN KEY (IdChoferTro) REFERENCES dbo.Choferes (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_VehiculosChofer_Vehiculos_IdVehiculo FOREIGN KEY (IdVehiculo) REFERENCES dbo.Vehiculos (Id) ON DELETE NO ACTION
    );
END;

GO


IF OBJECT_ID(N'dbo.ComprasCombustibleResumen', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComprasCombustibleResumen (
        Id int NOT NULL IDENTITY,
        Fecha datetime2 NOT NULL,
        Consecutivo int NOT NULL CONSTRAINT DF_ComprasCombustibleResumen_Consecutivo DEFAULT (0),
        FechaFactura datetime2 NOT NULL,
        NoFactura nvarchar(60) NOT NULL,
        IdProveedor int NOT NULL,
        IdMoneda int NOT NULL,
        Flete decimal(18,4) NOT NULL,
        THCD decimal(18,4) NOT NULL,
        Seguro decimal(18,4) NOT NULL,
        Terminada bit NOT NULL,
        FechaTerminada datetime2 NULL,
        Cancelada bit NOT NULL,
        FechaCancelada datetime2 NULL,
        Comentario nvarchar(max) NULL,
        CONSTRAINT PK_ComprasCombustibleResumen PRIMARY KEY (Id),
        CONSTRAINT FK_ComprasCombustibleResumen_Monedas_IdMoneda FOREIGN KEY (IdMoneda) REFERENCES dbo.Monedas (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_ComprasCombustibleResumen_Proveedores_IdProveedor FOREIGN KEY (IdProveedor) REFERENCES dbo.Proveedores (Id) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'dbo.ComprasCombustibleDetalle', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComprasCombustibleDetalle (
        Id int NOT NULL IDENTITY,
        IdCompra int NOT NULL,
        IdProducto int NOT NULL,
        IdMedida int NOT NULL,
        Precio AS (CONVERT(decimal(38,20), CASE WHEN Cantidad = 0 THEN 0 ELSE Importe / Cantidad END)),
        Cantidad decimal(18,2) NOT NULL,
        Importe decimal(18,2) NOT NULL,
        CONSTRAINT PK_ComprasCombustibleDetalle PRIMARY KEY (Id),
        CONSTRAINT FK_ComprasCombustibleDetalle_ComprasCombustibleResumen_IdCompra FOREIGN KEY (IdCompra) REFERENCES dbo.ComprasCombustibleResumen (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_ComprasCombustibleDetalle_TipoCombustible_IdProducto FOREIGN KEY (IdProducto) REFERENCES dbo.TipoCombustible (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_ComprasCombustibleDetalle_UnidadMedida_IdMedida FOREIGN KEY (IdMedida) REFERENCES dbo.UnidadMedida (Id) ON DELETE NO ACTION
    );
END;

GO


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoVehiculos_IdMarca' AND object_id = OBJECT_ID(N'dbo.TipoVehiculos'))
    CREATE INDEX IX_TipoVehiculos_IdMarca ON dbo.TipoVehiculos (IdMarca);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoVehiculos_IdModelo' AND object_id = OBJECT_ID(N'dbo.TipoVehiculos'))
    CREATE INDEX IX_TipoVehiculos_IdModelo ON dbo.TipoVehiculos (IdModelo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoVehiculos_IdCombustible' AND object_id = OBJECT_ID(N'dbo.TipoVehiculos'))
    CREATE INDEX IX_TipoVehiculos_IdCombustible ON dbo.TipoVehiculos (IdCombustible);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vehiculos_IdDeposito' AND object_id = OBJECT_ID(N'dbo.Vehiculos'))
    CREATE INDEX IX_Vehiculos_IdDeposito ON dbo.Vehiculos (IdDeposito);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vehiculos_IdCombustible' AND object_id = OBJECT_ID(N'dbo.Vehiculos'))
    CREATE INDEX IX_Vehiculos_IdCombustible ON dbo.Vehiculos (IdCombustible);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vehiculos_IdTipoServicio' AND object_id = OBJECT_ID(N'dbo.Vehiculos'))
    CREATE INDEX IX_Vehiculos_IdTipoServicio ON dbo.Vehiculos (IdTipoServicio);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vehiculos_IdTipoVehiculo' AND object_id = OBJECT_ID(N'dbo.Vehiculos'))
    CREATE INDEX IX_Vehiculos_IdTipoVehiculo ON dbo.Vehiculos (IdTipoVehiculo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentosVehiculo_ArchivoID' AND object_id = OBJECT_ID(N'dbo.DocumentosVehiculo'))
    CREATE INDEX IX_DocumentosVehiculo_ArchivoID ON dbo.DocumentosVehiculo (ArchivoID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentosVehiculo_IdTipoDocumento' AND object_id = OBJECT_ID(N'dbo.DocumentosVehiculo'))
    CREATE INDEX IX_DocumentosVehiculo_IdTipoDocumento ON dbo.DocumentosVehiculo (IdTipoDocumento);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentosVehiculo_IdVehiculo' AND object_id = OBJECT_ID(N'dbo.DocumentosVehiculo'))
    CREATE INDEX IX_DocumentosVehiculo_IdVehiculo ON dbo.DocumentosVehiculo (IdVehiculo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoDocumentoVehiculo_Nombre' AND object_id = OBJECT_ID(N'dbo.TipoDocumentoVehiculo'))
    CREATE UNIQUE INDEX IX_TipoDocumentoVehiculo_Nombre ON dbo.TipoDocumentoVehiculo (Nombre);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VehiculosChofer_IdChofer' AND object_id = OBJECT_ID(N'dbo.VehiculosChofer'))
    CREATE INDEX IX_VehiculosChofer_IdChofer ON dbo.VehiculosChofer (IdChofer);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VehiculosChofer_IdChoferSdo' AND object_id = OBJECT_ID(N'dbo.VehiculosChofer'))
    CREATE INDEX IX_VehiculosChofer_IdChoferSdo ON dbo.VehiculosChofer (IdChoferSdo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VehiculosChofer_IdChoferTro' AND object_id = OBJECT_ID(N'dbo.VehiculosChofer'))
    CREATE INDEX IX_VehiculosChofer_IdChoferTro ON dbo.VehiculosChofer (IdChoferTro);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VehiculosChofer_IdVehiculo' AND object_id = OBJECT_ID(N'dbo.VehiculosChofer'))
    CREATE INDEX IX_VehiculosChofer_IdVehiculo ON dbo.VehiculosChofer (IdVehiculo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprasCombustibleResumen_IdMoneda' AND object_id = OBJECT_ID(N'dbo.ComprasCombustibleResumen'))
    CREATE INDEX IX_ComprasCombustibleResumen_IdMoneda ON dbo.ComprasCombustibleResumen (IdMoneda);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprasCombustibleResumen_IdProveedor' AND object_id = OBJECT_ID(N'dbo.ComprasCombustibleResumen'))
    CREATE INDEX IX_ComprasCombustibleResumen_IdProveedor ON dbo.ComprasCombustibleResumen (IdProveedor);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprasCombustibleDetalle_IdCompra' AND object_id = OBJECT_ID(N'dbo.ComprasCombustibleDetalle'))
    CREATE INDEX IX_ComprasCombustibleDetalle_IdCompra ON dbo.ComprasCombustibleDetalle (IdCompra);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprasCombustibleDetalle_IdProducto' AND object_id = OBJECT_ID(N'dbo.ComprasCombustibleDetalle'))
    CREATE INDEX IX_ComprasCombustibleDetalle_IdProducto ON dbo.ComprasCombustibleDetalle (IdProducto);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ComprasCombustibleDetalle_IdMedida' AND object_id = OBJECT_ID(N'dbo.ComprasCombustibleDetalle'))
    CREATE INDEX IX_ComprasCombustibleDetalle_IdMedida ON dbo.ComprasCombustibleDetalle (IdMedida);

GO


IF OBJECT_ID(N'dbo.Vehiculos', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Vehiculos', N'IdDeposito') IS NULL
BEGIN
    ALTER TABLE dbo.Vehiculos ADD IdDeposito int NULL;
END;

IF COL_LENGTH(N'dbo.Vehiculos', N'IdDeposito') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vehiculos_Depositos_IdDeposito')
   AND NOT EXISTS (SELECT 1 FROM dbo.Vehiculos v WHERE v.IdDeposito IS NULL
                   OR NOT EXISTS (SELECT 1 FROM dbo.Depositos d WHERE d.Id = v.IdDeposito))
BEGIN
    ALTER TABLE dbo.Vehiculos ADD CONSTRAINT FK_Vehiculos_Depositos_IdDeposito
        FOREIGN KEY (IdDeposito) REFERENCES dbo.Depositos (Id) ON DELETE NO ACTION;
END;

IF COL_LENGTH(N'dbo.Vehiculos', N'IdDeposito') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.Vehiculos WHERE IdDeposito IS NULL)
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Vehiculos')
               AND name = N'IdDeposito' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Vehiculos ALTER COLUMN IdDeposito int NOT NULL;
END;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TipoVehiculos')
           AND name = N'Bateria' AND is_nullable = 1)
BEGIN
    UPDATE dbo.TipoVehiculos SET Bateria = N'' WHERE Bateria IS NULL;
    ALTER TABLE dbo.TipoVehiculos ALTER COLUMN Bateria nvarchar(100) NOT NULL;
END;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TipoVehiculos')
           AND name = N'Neumaticos' AND is_nullable = 1)
BEGIN
    UPDATE dbo.TipoVehiculos SET Neumaticos = N'' WHERE Neumaticos IS NULL;
    ALTER TABLE dbo.TipoVehiculos ALTER COLUMN Neumaticos nvarchar(100) NOT NULL;
END;

GO


IF OBJECT_ID(N'dbo.vw_ReporteVehiculos', N'V') IS NOT NULL
    DROP VIEW dbo.vw_ReporteVehiculos;

GO


CREATE VIEW dbo.vw_ReporteVehiculos
AS
SELECT
    v.Id,
    v.Matricula,
    v.Indicativo,
    tv.TipoVehiculo,
    m.Marca,
    mo.Modelos AS Modelo,
    ts.Operacion AS TipoServicio,
    dep.Deposito,
    tc.Combustible,
    v.IndiceReal,
    v.CapTanqueComb AS CapacidadTanque,
    v.NoCarroceria,
    v.NoMotor,
    tv.IndiceNormado,
    tv.Bateria,
    tv.Neumaticos,
    tv.NoPlazas,
    tv.CapCarga,
    tv.CapTanqueComb AS CapacidadTanqueNormada,
    STUFF((
        SELECT DISTINCT ', ' + c.NombreChofer
        FROM dbo.VehiculosChofer vc
        INNER JOIN dbo.Choferes c ON c.Id IN (vc.IdChofer, vc.IdChoferSdo, vc.IdChoferTro)
        WHERE vc.IdVehiculo = v.Id
        FOR XML PATH('')), 1, 2, '') AS Choferes
FROM dbo.Vehiculos v
LEFT JOIN dbo.TipoVehiculos tv ON tv.Id = v.IdTipoVehiculo
LEFT JOIN dbo.Marcas m ON m.Id = tv.IdMarca
LEFT JOIN dbo.Modelos mo ON mo.Id = tv.IdModelo
LEFT JOIN dbo.TipoServicios ts ON ts.Id = v.IdTipoServicio
LEFT JOIN dbo.Depositos dep ON dep.Id = v.IdDeposito
LEFT JOIN dbo.TipoCombustible tc ON tc.Id = v.IdCombustible;

GO


IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = OBJECT_ID(N'dbo.ComprasCombustibleResumen'))
BEGIN
    EXEC(N'CREATE TRIGGER dbo.TR_ComprasCombustibleResumen_Consecutivo
ON dbo.ComprasCombustibleResumen
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Pendientes AS (
        SELECT i.Id, ROW_NUMBER() OVER (ORDER BY i.Id) AS rn
        FROM inserted i
        WHERE i.Consecutivo = 0
    )
    UPDATE c
    SET c.Consecutivo = base.Actual + p.rn
    FROM dbo.ComprasCombustibleResumen c
    INNER JOIN Pendientes p ON p.Id = c.Id
    CROSS APPLY (
        SELECT ISNULL(MAX(x.Consecutivo), 0) AS Actual
        FROM dbo.ComprasCombustibleResumen x
        WHERE NOT EXISTS (SELECT 1 FROM inserted i2 WHERE i2.Id = x.Id)
    ) AS base;
END');
END;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261023171008_BaselineBusinessSchema', N'5.0.5');
GO

COMMIT;
GO

