using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using DevExpress.Persistent.BaseImpl.EF.Kpi;

namespace DXComnTransp.Module.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891.
public class DXComnTranspContextInitializer : DbContextTypesInfoInitializerBase {
	protected override DbContext CreateDbContext() {
		var optionsBuilder = new DbContextOptionsBuilder<DXComnTranspEFCoreDbContext>()
            .UseSqlServer(@";");
        return new DXComnTranspEFCoreDbContext(optionsBuilder.Options);
	}
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class DXComnTranspDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<DXComnTranspEFCoreDbContext>
{
    public DXComnTranspEFCoreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<DXComnTranspEFCoreDbContext>();

        optionsBuilder.UseSqlServer(GetConnectionString());

        return new DXComnTranspEFCoreDbContext(
            optionsBuilder.Options
        );
    }

    private static string GetConnectionString()
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = Environment.GetEnvironmentVariable("DXCOMNTRANSP_CONNECTIONSTRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=COMBTRANSP;Integrated Security=SSPI;TrustServerCertificate=True";
        return connectionString;
    }
}

[TypesInfoInitializer(typeof(DXComnTranspContextInitializer))]
public class DXComnTranspEFCoreDbContext : DbContext {
	public DXComnTranspEFCoreDbContext(DbContextOptions<DXComnTranspEFCoreDbContext> options) : base(options) {
	}
	public DbSet<ModuleInfo> ModulesInfo { get; set; }
	public DbSet<ModelDifference> ModelDifferences { get; set; }
	public DbSet<ModelDifferenceAspect> ModelDifferenceAspects { get; set; }
	public DbSet<PermissionPolicyRole> Roles { get; set; }
	public DbSet<DXComnTransp.Module.BusinessObjects.ApplicationUser> Users { get; set; }
    public DbSet<DXComnTransp.Module.BusinessObjects.ApplicationUserLoginInfo> UserLoginInfos { get; set; }
	public DbSet<FileData> FileData { get; set; }
	public DbSet<ReportDataV2> ReportDataV2 { get; set; }
	public DbSet<KpiDefinition> KpiDefinition { get; set; }
	public DbSet<KpiInstance> KpiInstance { get; set; }
	public DbSet<KpiHistoryItem> KpiHistoryItem { get; set; }
	public DbSet<KpiScorecard> KpiScorecard { get; set; }
	public DbSet<DashboardData> DashboardData { get; set; }
    public DbSet<AuditDataItemPersistent> AuditData { get; set; }
    public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReference { get; set; }

    // ============================================================
    // COMBTRANSP - CATÁLOGOS
    // ============================================================

    public DbSet<TipoCombustible> TipoCombustibles { get; set; }
    public DbSet<UnidadMedida> UnidadesMedida { get; set; }
    public DbSet<Moneda> Monedas { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Deposito> Depositos { get; set; }
    public DbSet<TipoMovimientoCombustible> TiposMovimientoCombustible { get; set; }
    public DbSet<TipoInventario> TiposInventario { get; set; }
    public DbSet<TipoServicio> TiposServicio { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Modelo> Modelos { get; set; }
    public DbSet<Chofer> Choferes { get; set; }
    public DbSet<TipoVehiculo> TiposVehiculo { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<TipoDocumentoVehiculo> TiposDocumentoVehiculo { get; set; }
    public DbSet<DocumentoVehiculo> DocumentosVehiculo { get; set; }
    public DbSet<VehiculoChofer> VehiculosChofer { get; set; }
    public DbSet<CompraCombustible> ComprasCombustible { get; set; }
    public DbSet<CompraCombustibleDetalle> ComprasCombustibleDetalles { get; set; }
    public DbSet<EntradaCombustible> EntradasCombustible { get; set; }
    public DbSet<EntradaCombustibleDetalle> EntradasCombustibleDetalles { get; set; }
    public DbSet<Inventario> Inventarios { get; set; }
    public DbSet<ReporteVehiculo> ReportesVehiculos { get; set; }
    public DbSet<Configuracion> Configuraciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============================================================
        // XAF SECURITY
        // ============================================================

        modelBuilder.Entity<ApplicationUserLoginInfo>(b =>
        {
            b.HasIndex(
                nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.LoginProviderName),
                nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.ProviderUserKey)
            ).IsUnique();
        });


        // ============================================================
        // COMBTRANSP - CATÁLOGOS
        // ============================================================

        modelBuilder.Entity<TipoCombustible>(b =>
        {
            b.Property(x => x.Combustible)
                .HasMaxLength(60);

            b.Property(x => x.PCosto)
                .HasPrecision(18, 2);

            b.Property(x => x.PVenta)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<UnidadMedida>(b =>
        {
            b.Property(x => x.UM)
                .HasMaxLength(8);
        });

        modelBuilder.Entity<Moneda>(b =>
        {
            b.Property(x => x.Nombre)
                .HasMaxLength(6);

            b.Property(x => x.TasaCambio)
                .HasPrecision(18, 4);
        });

        modelBuilder.Entity<Proveedor>(b =>
        {
            b.Property(x => x.Nombre)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Deposito>(b =>
        {
            b.Property(x => x.Nombre)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<TipoMovimientoCombustible>(b =>
        {
            b.Property(x => x.Codigo)
                .HasColumnType("varchar(30)");

            b.Property(x => x.Descripcion)
                .HasColumnType("varchar(100)");
        });

        modelBuilder.Entity<TipoInventario>(b =>
        {
            b.Property(x => x.TpoInventario)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<TipoServicio>(b =>
        {
            b.Property(x => x.Operacion)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Marca>(b =>
        {
            b.Property(x => x.Nombre)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Modelo>(b =>
        {
            b.Property(x => x.Modelos)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Chofer>(b =>
        {
            b.Property(x => x.CIdentidad)
                .HasMaxLength(30);

            b.Property(x => x.NombreChofer)
                .HasMaxLength(100);

            b.Property(x => x.Licencias)
                .HasMaxLength(100);

            b.Property(x => x.Telefono)
                .HasMaxLength(100);
        });


        // ============================================================
        // VEHÍCULOS
        // ============================================================

        modelBuilder.Entity<TipoVehiculo>(b =>
        {
            b.Property(x => x.TipoVehiculoNombre)
                .HasMaxLength(100);

            b.Property(x => x.Bateria)
                .HasMaxLength(100);

            b.Property(x => x.Neumaticos)
                .HasMaxLength(100);

            b.Property(x => x.IndiceNormado)
                .HasPrecision(18, 2);

            b.Property(x => x.CapTanqueComb)
                .HasPrecision(18, 2);

            b.Property(x => x.CapCarga)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Vehiculo>(b =>
        {
            b.Property(x => x.Matricula)
                .HasMaxLength(20);

            b.Property(x => x.Indicativo)
                .HasMaxLength(100);

            b.Property(x => x.NoCarroceria)
                .HasMaxLength(100);

            b.Property(x => x.NoMotor)
                .HasMaxLength(100);

            b.Property(x => x.IndiceReal)
                .HasPrecision(18, 2);

            b.Property(x => x.CapTanqueComb)
                .HasPrecision(18, 2);

            b.HasOne(x => x.Deposito)
           .WithMany()
           .HasForeignKey(x => x.IdDeposito)
           .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Documentos)
                .WithOne(x => x.Vehiculo)
                .HasForeignKey(x => x.IdVehiculo)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TipoDocumentoVehiculo>(b =>
        {
            b.Property(x => x.Nombre)
                .HasMaxLength(100);

            b.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        modelBuilder.Entity<DocumentoVehiculo>(b =>
        {
            b.Property(x => x.Descripcion)
                .HasMaxLength(200);

            b.Property(x => x.Observaciones)
                .HasMaxLength(500);

            b.HasOne(x => x.TipoDocumento)
                .WithMany()
                .HasForeignKey(x => x.IdTipoDocumento)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Archivo)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VehiculoChofer>(b =>
        {
            b.HasOne(x => x.Vehiculo)
                .WithMany()
                .HasForeignKey(x => x.IdVehiculo)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Chofer)
                .WithMany()
                .HasForeignKey(x => x.IdChofer)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ChoferSdo)
                .WithMany()
                .HasForeignKey(x => x.IdChoferSdo)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ChoferTro)
                .WithMany()
                .HasForeignKey(x => x.IdChoferTro)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.SysStartTime)
                .ValueGeneratedOnAddOrUpdate();

            b.Property(x => x.SysEndTime)
                .ValueGeneratedOnAddOrUpdate();
        });


        // ============================================================
        // COMPRAS
        // ============================================================

        modelBuilder.Entity<CompraCombustible>(b =>
        {
            b.Property(x => x.Flete)
                .HasPrecision(18, 4);

            b.Property(x => x.THCD)
                .HasPrecision(18, 4);

            b.Property(x => x.Seguro)
                .HasPrecision(18, 4);

            b.Property(x => x.NoFactura)
                .HasMaxLength(60);

            b.HasOne(x => x.Proveedor)
                .WithMany()
                .HasForeignKey(x => x.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Moneda)
                .WithMany()
                .HasForeignKey(x => x.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompraCombustibleDetalle>(b =>
        {
            b.Property(x => x.NoContenedor)
                .HasMaxLength(11);

            b.Property(x => x.Precio)
                .HasPrecision(38, 20);

            b.Property(x => x.Cantidad)
                .HasPrecision(18, 2);

            b.Property(x => x.Importe)
                .HasPrecision(18, 2);

            b.HasOne(x => x.Compra)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.IdCompra)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Medida)
                .WithMany()
                .HasForeignKey(x => x.IdMedida)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // ENTRADAS DE COMBUSTIBLE (tabla legada RecepcionResumen /
        // RecepcionDetalle; las tablas YA existen en la BD, no
        // generar migración para ellas)
        // ============================================================

        modelBuilder.Entity<EntradaCombustible>(b =>
        {
            b.Property(x => x.NoFactura)
                .HasMaxLength(50);

            b.Property(x => x.Comentario)
                .HasMaxLength(4000);

            b.Property(x => x.TasaCambio)
                .HasPrecision(18, 4);

            b.HasOne(x => x.Deposito)
                .WithMany()
                .HasForeignKey(x => x.IdDeposito)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Proveedor)
                .WithMany()
                .HasForeignKey(x => x.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Moneda)
                .WithMany()
                .HasForeignKey(x => x.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntradaCombustibleDetalle>(b =>
        {
            // La FK del resumen tiene ON DELETE CASCADE en la BD legada.
            b.Property(x => x.Precio)
                .HasPrecision(18, 4);

            b.Property(x => x.TasaCambio)
                .HasPrecision(18, 4);

            b.Property(x => x.Cantidad)
                .HasPrecision(18, 2);

            b.HasOne(x => x.Entrada)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.IdRecepcion)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Medida)
                .WithMany()
                .HasForeignKey(x => x.IdMedida)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // INVENTARIO (tabla legada Inventarios, ya existe en la BD)
        // ============================================================

        modelBuilder.Entity<Inventario>(b =>
        {
            b.Property(x => x.Precio)
                .HasPrecision(18, 4);

            b.Property(x => x.Existencia)
                .HasPrecision(18, 4);

            b.HasOne(x => x.Deposito)
                .WithMany()
                .HasForeignKey(x => x.IdDeposito)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TipoInventario)
                .WithMany()
                .HasForeignKey(x => x.IdTipoInv)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReporteVehiculo>(b =>
        {
            b.ToView("vw_ReporteVehiculos", "dbo");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("Id").HasColumnType("bigint");
            b.Property(x => x.Matricula).HasColumnName("Matricula");
            b.Property(x => x.Indicativo).HasColumnName("Indicativo");
            b.Property(x => x.TipoVehiculo).HasColumnName("TipoVehiculo");
            b.Property(x => x.Marca).HasColumnName("Marca");
            b.Property(x => x.Modelo).HasColumnName("Modelo");
            b.Property(x => x.TipoServicio).HasColumnName("TipoServicio");
            b.Property(x => x.Deposito).HasColumnName("Deposito");
            b.Property(x => x.Combustible).HasColumnName("Combustible");
            b.Property(x => x.IndiceReal).HasColumnName("IndiceReal");
            b.Property(x => x.CapacidadTanque).HasColumnName("CapacidadTanque");
            b.Property(x => x.NoCarroceria).HasColumnName("NoCarroceria");
            b.Property(x => x.NoMotor).HasColumnName("NoMotor");
            b.Property(x => x.IndiceNormado).HasColumnName("IndiceNormado");
            b.Property(x => x.Bateria).HasColumnName("Bateria");
            b.Property(x => x.Neumaticos).HasColumnName("Neumaticos");
            b.Property(x => x.NoPlazas).HasColumnName("NoPlazas");
            b.Property(x => x.CapCarga).HasColumnName("CapCarga");
            b.Property(x => x.CapacidadTanqueNormada)
                .HasColumnName("CapacidadTanqueNormada");
            b.Property(x => x.Choferes).HasColumnName("Choferes");
            b.Ignore(x => x.DescripcionCompleta);
        });

        modelBuilder.Entity<Configuracion>(b =>
        {
            b.Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar(200)")
                .IsRequired();

            b.Property(x => x.Slogan)
                .HasColumnName("slogan")
                .HasColumnType("varchar(250)");

            b.Property(x => x.Logo)
                .HasColumnName("logo")
                .HasColumnType("image");

            b.Property(x => x.Nit)
                .HasColumnName("nit")
                .HasColumnType("varchar(50)")
                .IsRequired();

            b.Property(x => x.Reeup)
                .HasColumnName("reeup")
                .HasColumnType("varchar(50)");

            b.Property(x => x.CuentaBancaria)
                .HasColumnName("cuenta_bancaria")
                .HasColumnType("varchar(100)");

            b.Property(x => x.Correo)
                .HasColumnName("correo")
                .HasColumnType("varchar(160)");

            b.Property(x => x.Telefonos)
                .HasColumnName("telefonos")
                .HasColumnType("text");

            b.Property(x => x.TasaVigente)
                .HasColumnName("tasa_vigente")
                .HasPrecision(12, 4)
                .IsRequired();

            b.Property(x => x.PathSalvaBD)
                .HasColumnName("PathSalvaBD")
                .HasColumnType("nvarchar(250)");
        });
    }
}

public class DXComnTranspAuditingDbContext : DbContext {
    public DXComnTranspAuditingDbContext(DbContextOptions<DXComnTranspAuditingDbContext> options) : base(options) {
    }
    public DbSet<AuditDataItemPersistent> AuditData { get; set; }
    public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReference { get; set; }
}
