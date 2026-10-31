using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using DXComnTransp.Blazor.Server.Services;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.Identity.Web;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OData;
using DevExpress.ExpressApp.WebApi.Services;
using DXComnTransp.WebApi.JWT;
using DevExpress.ExpressApp.Security.Authentication;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.Core;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace DXComnTransp.Blazor.Server;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {

        var supportedCultures = new[]
            {
                new CultureInfo("es-MX")
            };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("es-MX");

            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<DXComnTranspBlazorApplication>();
            builder.Modules
                .AddAuditTrailEFCore()
                .AddConditionalAppearance()
                .AddDashboards(options => {
                    options.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.EF.DashboardData);
                })
                .AddFileAttachments()
                .AddOffice()
                .AddReports(options => {
                    options.EnableInplaceReports = true;
                    options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
                    options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                })
                .AddValidation(options => {
                    options.AllowValidationDetailsAccess = false;
                })
                .AddViewVariants(options => {
                    options.ShowAdditionalNavigation = true;
                })
                .Add<DXComnTransp.Module.DXComnTranspModule>()
            	.Add<DXComnTranspBlazorModule>();
            builder.ObjectSpaceProviders
                .AddSecuredEFCore().WithAuditedDbContext(contexts => {
                    contexts.Configure<DXComnTransp.Module.BusinessObjects.DXComnTranspEFCoreDbContext, DXComnTransp.Module.BusinessObjects.DXComnTranspAuditingDbContext>(
                        (serviceProvider, businessObjectDbContextOptions) => {
                            // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                            // Do not use this code in production environment to avoid data loss.
                            // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                            //businessObjectDbContextOptions.UseInMemoryDatabase("InMemory");
                            string connectionString = GetRequiredConnectionString();
                            businessObjectDbContextOptions.UseSqlServer(connectionString);
                            businessObjectDbContextOptions.UseLazyLoadingProxies();
                        },
                        (serviceProvider, auditHistoryDbContextOptions) => {
                            string connectionString = GetRequiredConnectionString();
                            auditHistoryDbContextOptions.UseSqlServer(connectionString);
                            auditHistoryDbContextOptions.UseLazyLoadingProxies();
                        });
                })
                .AddNonPersistent();
            builder.Security
                .UseIntegratedMode(options => {
                    options.RoleType = typeof(PermissionPolicyRole);
                    // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                    // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                    options.UserType = typeof(DXComnTransp.Module.BusinessObjects.ApplicationUser);
                    // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                    // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                    options.UserLoginInfoType = typeof(DXComnTransp.Module.BusinessObjects.ApplicationUserLoginInfo);
                })
                .AddPasswordAuthentication(options => {
                    options.IsSupportChangePassword = true;
                })
                .AddExternalAuthentication(options => {
                    options.Events.OnAuthenticated = (externalAuthenticationContext) => {
                        // La creación automática de usuarios desde proveedores externos está deshabilitada:
                        // las cuentas deben provisionarse explícitamente por un administrador.
                        // Para habilitarla, consulte https://docs.devexpress.com/eXpressAppFramework/402197
                    };
                });
        });
        const string customBearerSchemeName = "CustomBearer";
        var authentication = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
        authentication
            .AddCookie(options => {
                options.LoginPath = "/LoginPage";
            })
            .AddJwtBearer(customBearerSchemeName, options => {
                string issuer = Configuration["Authentication:Jwt:Issuer"];
                string audience = Configuration["Authentication:Jwt:Audience"];
                string signingKey = Configuration["Authentication:Jwt:IssuerSigningKey"];
                if(string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)) {
                    throw new InvalidOperationException(
                        "Configure 'Authentication:Jwt:Issuer' y 'Authentication:Jwt:Audience'.");
                }
                if(string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32) {
                    throw new InvalidOperationException(
                        "Configure 'Authentication:Jwt:IssuerSigningKey' con al menos 32 caracteres. " +
                        "Use la variable de entorno 'Authentication__Jwt__IssuerSigningKey' o user-secrets.");
                }
                options.TokenValidationParameters = new TokenValidationParameters() {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        //Configure OAuth2 Identity Providers based on your requirements. For more information, see
        //https://docs.devexpress.com/eXpressAppFramework/402197/task-based-help/security/how-to-use-active-directory-and-oauth2-authentication-providers-in-blazor-applications
        //https://developers.google.com/identity/protocols/oauth2
        //https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow
        //https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow
        authentication.AddMicrosoftIdentityWebApp(Configuration, configSectionName: "Authentication:AzureAd", cookieScheme: null);
        authentication.AddMicrosoftIdentityWebApi(Configuration, configSectionName: "Authentication:AzureAd");

        services.AddAuthorization(options => {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                customBearerSchemeName)
                    .RequireAuthenticatedUser()
                    .RequireXafAuthentication()
                    .Build();
        });

        services
            .AddXafWebApi(Configuration, options => {
                // Use options.BusinessObject<YourBusinessObject>() to make the Business Object available in the Web API and generate the GET, POST, PUT, and DELETE HTTP methods for it.
            });
        services
            .AddControllers()
            .AddOData((options, serviceProvider) => {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel())
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c => {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "DXComnTransp API",
                Version = "v1",
                Description = @"Use AddXafWebApi(options) in the DXComnTransp.Blazor.Server\Startup.cs file to make Business Objects available in the Web API."
            });
            c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme() {
                Type = SecuritySchemeType.Http,
                Name = "Bearer",
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme() {
                            Reference = new OpenApiReference() {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "JWT"
                            }
                        },
                        new string[0]
                    },
            });
            var azureAdAuthorityUrl = $"{Configuration["Authentication:AzureAd:Instance"]}{Configuration["Authentication:AzureAd:TenantId"]}";
            c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows() {
                    AuthorizationCode = new OpenApiOAuthFlow() {
                        AuthorizationUrl = new Uri($"{azureAdAuthorityUrl}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"{azureAdAuthorityUrl}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string> {
                            // Configure scopes corresponding to https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis
                            { @"[Enter the scope name in the DXComnTransp.Blazor.Server\Startup.cs file]", @"[Enter the scope description in the DXComnTransp.Blazor.Server\Startup.cs file]" }
                        }
                    }
                }
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                {
                    new OpenApiSecurityScheme {
                        Name = "OAuth2",
                        Scheme = "OAuth2",
                        Reference = new OpenApiReference {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "OAuth2"
                        },
                        In = ParameterLocation.Header
                    },
                    new string[0]
                }
            });
        });
    }

    private string GetRequiredConnectionString() {
#if EASYTEST
        string easyTestConnectionString = Configuration.GetConnectionString("EasyTestConnectionString");
        if(!string.IsNullOrWhiteSpace(easyTestConnectionString)) {
            return easyTestConnectionString;
        }
#endif
        string connectionString = Configuration.GetConnectionString("ConnectionString");
        if(string.IsNullOrWhiteSpace(connectionString)) {
            connectionString = Environment.GetEnvironmentVariable("DXCOMNTRANSP_CONNECTIONSTRING");
        }
        if(string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException(
                "No se encontró la cadena de conexión. Defina la variable de entorno " +
                "'ConnectionStrings__ConnectionString' (o 'DXCOMNTRANSP_CONNECTIONSTRING') " +
                "o ejecute: dotnet user-secrets set \"ConnectionStrings:ConnectionString\" \"<cadena>\".");
        }
        return connectionString;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DXComnTransp WebApi v1");
                c.OAuthClientId(Configuration["Authentication:AzureAd:ClientId"]);
                c.OAuthUsePkce();
            });
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
