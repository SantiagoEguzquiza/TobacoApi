using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.Mapping;
using TobacoBackend.Repositories;
using TobacoBackend.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using TobacoBackend.Middleware;
using AspNetCoreRateLimit;
using TobacoBackend.Helpers;
using System.Text.Json;
using TobacoBackend.Authorization;
using TobacoBackend.Domain.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });

// Swagger habilitado en todos los entornos
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Tobaco API", Version = "v1" });
    
    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();

// Database - Usar factory para inyectar IHttpContextAccessor
builder.Services.AddDbContext<AplicationDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    // El DbContext se crea con el constructor que acepta IHttpContextAccessor
}, ServiceLifetime.Scoped);

// Registrar servicios
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVentaPagoService, VentaPagoService>();
builder.Services.AddScoped<IPrecioEspecialService, PrecioEspecialService>();
builder.Services.AddScoped<IAbonosService, AbonosService>();
builder.Services.AddScoped<IProductoAFavorService, ProductoAFavorService>();
builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
builder.Services.AddScoped<IRecorridoProgramadoService, RecorridoProgramadoService>();
builder.Services.AddScoped<IPermisosEmpleadoService, PermisosEmpleadoService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<SecurityLoggingService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AccountLockoutService>();
builder.Services.AddHttpClient();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Backup Service (Hosted Service para backups automáticos)
var backupEnabled = builder.Configuration.GetValue<bool>("BackupSettings:Enabled", true);
if (backupEnabled)
{
    builder.Services.AddHostedService<BackupService>();
}

// Keep-alive de la base de datos: ping cada 4 min para evitar cold start tras inactividad (producción)
builder.Services.AddHostedService<DatabaseKeepAliveService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<SystemHealthCheck>("system");

// Registrar repositorios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVentaPagoRepository, VentaPagoRepository>();
builder.Services.AddScoped<IPrecioEspecialRepository, PrecioEspecialRepository>();
builder.Services.AddScoped<IAbonosRepository, AbonosRepository>();
builder.Services.AddScoped<IProductoAFavorRepository, ProductoAFavorRepository>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
builder.Services.AddScoped<IRecorridoProgramadoRepository, RecorridoProgramadoRepository>();
builder.Services.AddScoped<IPermisosEmpleadoRepository, PermisosEmpleadoRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// CORS - Configuración restrictiva
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" }; // Valores por defecto para desarrollo

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebapp", builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowCredentials()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Rate Limiting - Prevenir ataques de fuerza bruta
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/User/login",
            Period = "1m",
            Limit = 5 // Máximo 5 intentos de login por minuto
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100 // Límite general de 100 requests por minuto
        }
    };
});

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configuración de JWT - Usar variables de entorno
// IMPORTANTE: Primero obtener el secret correcto ANTES de configurar IOptions
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? builder.Configuration["JwtSettings:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret no configurado. Configure la variable de entorno JWT_SECRET o en appsettings.json");

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret debe tener al menos 32 caracteres para ser seguro.");
}

// Crear jwtSettings con el secret correcto
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
jwtSettings.Secret = jwtSecret; // Usar el secret de variable de entorno o appsettings

// Configurar IOptions<JwtSettings> con el secret correcto
builder.Services.Configure<JwtSettings>(options =>
{
    builder.Configuration.GetSection("JwtSettings").Bind(options);
    options.Secret = jwtSecret; // Asegurar que TokenService use el mismo secret
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
       .AddJwtBearer(options =>
       {
           options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Requerir HTTPS en producción
           options.SaveToken = true;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
               ValidateIssuer = true,
               ValidIssuer = jwtSettings.Issuer,
               ValidateAudience = true,
               ValidAudience = jwtSettings.Audience,
               ValidateLifetime = true,
               ClockSkew = TimeSpan.Zero,
               // Mapear el claim "sub" a NameIdentifier para compatibilidad
               NameClaimType = ClaimTypes.NameIdentifier,
               RoleClaimType = ClaimTypes.Role
           };
           
           // Eventos para debugging
           options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
           {
               OnAuthenticationFailed = context =>
               {
                   var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                   logger.LogError(context.Exception, "Error en autenticación JWT");
                   return Task.CompletedTask;
               },
               OnTokenValidated = context =>
               {
                   // Asegurar que el claim "sub" esté disponible como NameIdentifier
                   var subClaim = context.Principal?.FindFirst("sub");
                   if (subClaim != null)
                   {
                       var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                       if (identity != null && !identity.HasClaim(ClaimTypes.NameIdentifier, subClaim.Value))
                       {
                           identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
                       }
                   }
                   return Task.CompletedTask;
               }
           };
       });

// Configurar políticas de autorización basadas en roles
builder.Services.AddAuthorization(options =>
{
    // Solo SuperAdmin
    options.AddPolicy(AuthorizationPolicies.SuperAdminOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "SuperAdmin" })));

    // Solo Admin
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Admin" })));

    // SuperAdmin o Admin
    options.AddPolicy(AuthorizationPolicies.SuperAdminOrAdmin, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "SuperAdmin", "Admin" })));

    // Solo Employee
    options.AddPolicy(AuthorizationPolicies.EmployeeOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Employee" })));

    // Admin o Employee (cualquiera) - permite SuperAdmin también
    options.AddPolicy(AuthorizationPolicies.AdminOrEmployee, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "SuperAdmin", "Admin", "Employee" })));

    // Admin o Employee SOLO - EXCLUYE SuperAdmin (para datos de clientes)
    options.AddPolicy(AuthorizationPolicies.AdminOrEmployeeOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Admin", "Employee" })));

    // Solo Vendedor (Employee con TipoVendedor = Vendedor)
    options.AddPolicy(AuthorizationPolicies.VendedorOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Employee" }, new[] { TipoVendedor.Vendedor })));

    // Solo Repartidor (Employee con TipoVendedor = Repartidor)
    options.AddPolicy(AuthorizationPolicies.RepartidorOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Employee" }, new[] { TipoVendedor.Repartidor })));

    // Solo RepartidorVendedor (Employee con TipoVendedor = RepartidorVendedor)
    options.AddPolicy(AuthorizationPolicies.RepartidorVendedorOnly, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Employee" }, new[] { TipoVendedor.RepartidorVendedor })));

    // Admin o Vendedor
    options.AddPolicy(AuthorizationPolicies.AdminOrVendedor, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Admin", "Employee" }, new[] { TipoVendedor.Vendedor })));

    // Admin o Repartidor
    options.AddPolicy(AuthorizationPolicies.AdminOrRepartidor, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Admin", "Employee" }, new[] { TipoVendedor.Repartidor })));

    // Admin o RepartidorVendedor
    options.AddPolicy(AuthorizationPolicies.AdminOrRepartidorVendedor, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Admin", "Employee" }, new[] { TipoVendedor.RepartidorVendedor })));

    // Vendedor o RepartidorVendedor (pueden crear ventas)
    options.AddPolicy(AuthorizationPolicies.VendedorOrRepartidorVendedor, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Employee" }, new[] { TipoVendedor.Vendedor, TipoVendedor.RepartidorVendedor })));

    // Ver ventas: Admin, Vendedor o RepartidorVendedor (NO Repartidor)
    options.AddPolicy(AuthorizationPolicies.CanViewVentas, policy =>
        policy.Requirements.Add(new RoleRequirement(new[] { "Admin", "Employee" }, new[] { TipoVendedor.Vendedor, TipoVendedor.RepartidorVendedor })));
});

// Registrar el handler de autorización
// AuthorizationHandler<T> ya implementa IAuthorizationHandler, pero necesitamos registrarlo explícitamente
// Usar AddScoped con el tipo concreto y luego agregarlo como IAuthorizationHandler
builder.Services.AddScoped<RoleRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler>(sp => sp.GetRequiredService<RoleRequirementHandler>());

var app = builder.Build();

// Warm-up de la base de datos al arrancar: completar antes de aceptar tráfico para que
// el primer usuario no sufra cold start (p. ej. Azure SQL tarda 10-15 s la primera vez).
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
    await db.Database.CanConnectAsync(cts.Token);
    app.Logger.LogInformation("Base de datos: conexión lista (warm-up completado). API lista para producción.");
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Warm-up de base de datos falló; la API arranca igual. La primera petición puede ser lenta.");
}

// Configure the HTTP request pipeline

// Exception Handling debe ir primero
app.UseExceptionHandling();

// Request Logging (opcional, puede deshabilitarse en producción si es muy verboso)
if (app.Environment.IsDevelopment())
{
    app.UseRequestLogging();
}

// Swagger habilitado en todos los entornos
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    // En producción, forzar HTTPS
    app.UseHsts();
}

// Input Validation (debe ir antes de otros middlewares)
app.UseInputValidation();

// Security Headers
app.UseSecurityHeaders();

// Rate Limiting
app.UseIpRateLimiting();

// CORS
app.UseCors("AllowWebapp");

// HTTPS Redirection (solo en producción)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Health Checks endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            }),
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapControllers();

app.Run();
