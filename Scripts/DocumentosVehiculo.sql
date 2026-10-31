IF OBJECT_ID(N'dbo.TipoDocumentoVehiculo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TipoDocumentoVehiculo
    (
        Id INT IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_TipoDocumentoVehiculo PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        RequiereVencimiento BIT NOT NULL
            CONSTRAINT DF_TipoDocumentoVehiculo_RequiereVencimiento DEFAULT (0),
        Activo BIT NOT NULL
            CONSTRAINT DF_TipoDocumentoVehiculo_Activo DEFAULT (1)
    );

    CREATE UNIQUE INDEX UX_TipoDocumentoVehiculo_Nombre
        ON dbo.TipoDocumentoVehiculo (Nombre);
END
GO

IF OBJECT_ID(N'dbo.DocumentosVehiculo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentosVehiculo
    (
        Id BIGINT IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_DocumentosVehiculo PRIMARY KEY,
        IdVehiculo BIGINT NOT NULL,
        IdTipoDocumento INT NOT NULL,
        Descripcion NVARCHAR(200) NULL,
        FechaEmision DATETIME2 NULL,
        FechaVencimiento DATETIME2 NULL,
        Observaciones NVARCHAR(500) NULL,
        ArchivoID INT NULL
    );

    ALTER TABLE dbo.DocumentosVehiculo
        ADD CONSTRAINT FK_DocumentosVehiculo_Vehiculos
        FOREIGN KEY (IdVehiculo) REFERENCES dbo.Vehiculos (Id);

    ALTER TABLE dbo.DocumentosVehiculo
        ADD CONSTRAINT FK_DocumentosVehiculo_TipoDocumento
        FOREIGN KEY (IdTipoDocumento) REFERENCES dbo.TipoDocumentoVehiculo (Id);

    ALTER TABLE dbo.DocumentosVehiculo
        ADD CONSTRAINT FK_DocumentosVehiculo_FileData
        FOREIGN KEY (ArchivoID) REFERENCES dbo.FileData (ID);

    CREATE INDEX IX_DocumentosVehiculo_IdVehiculo
        ON dbo.DocumentosVehiculo (IdVehiculo);

    CREATE INDEX IX_DocumentosVehiculo_IdTipoDocumento
        ON dbo.DocumentosVehiculo (IdTipoDocumento);
END
GO

DECLARE @Inspeccion NVARCHAR(100) = N'Inspecci' + NCHAR(243) + N'n t' + NCHAR(233) + N'cnica';
DECLARE @Foto NVARCHAR(100) = N'Foto del veh' + NCHAR(237) + N'culo';

MERGE dbo.TipoDocumentoVehiculo AS destino
USING (VALUES
    (@Inspeccion, 1, 1),
    (N'Licencia operativa', 1, 1),
    (N'Contrato', 1, 1),
    (@Foto, 0, 1)
) AS origen (Nombre, RequiereVencimiento, Activo)
    ON destino.Nombre = origen.Nombre
WHEN NOT MATCHED THEN
    INSERT (Nombre, RequiereVencimiento, Activo)
    VALUES (origen.Nombre, origen.RequiereVencimiento, origen.Activo);
GO
