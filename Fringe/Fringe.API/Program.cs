using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Text;
using Fringe.Domain;
using Fringe.Domain.Entities;
using Fringe.Domain.Extensions;
using Fringe.Domain.Models;
using Fringe.Domain.Seeders;
using Fringe.Repository;
using Fringe.Repository.Interfaces;
using Fringe.Service;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Fringe.Service.Interfaces;
using Fringe.Service;
using Fringe.Repository.Interfaces;
using Fringe.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Identity service
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // User settings
        options.User.RequireUniqueEmail = true;
        
        // Lockout settings
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        
    })
    .AddEntityFrameworkStores<FringeDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JwtSettings configuration is missing or invalid");
}
var key = Encoding.ASCII.GetBytes(jwtSettings.Key);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireManagerRole", policy => 
        policy.RequireRole("Manager"));
    
    options.AddPolicy("RequireUserRole", policy => 
        policy.RequireRole("User"));
    
    options.AddPolicy("RequireAdminOrManagerRole", policy => 
        policy.RequireRole("Admin", "Manager"));
});

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy",
        policy => policy
            .WithOrigins(
                //Local development
                "http://localhost:3000",
                "http://localhost:3001",
                "http://127.0.0.1:1880/",
                //Production
                "https://fringebooking-dev.azurewebsites.net",
                "https://fringebooking-uat.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add database context
builder.Services.AddDbContext<FringeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register repositories
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<ITicketPriceRepository, TicketPriceRepository>();

//builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IUserQueryRepository, UserQueryRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IReservedSeatRepository, ReservedSeatRepository>();


//Report
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();



// Register services
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));
builder.Services.AddScoped<IExploreService, ExploreService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IShowService, ShowService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<ITicketPriceService, TicketPriceService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();


// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fringe API",
        Version = "v1",
        Description = "API for Fringe application"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //AutoMapper registration to map objects


// Configure Serilog
var columnOptions = new ColumnOptions();
columnOptions.Store.Add(StandardColumn.LogEvent);
columnOptions.AdditionalColumns = new Collection<SqlColumn>
{
    new SqlColumn { ColumnName = "Application", PropertyName = "Application", DataType = SqlDbType.NVarChar, DataLength = 50 },
    new SqlColumn { ColumnName = "Environment", PropertyName = "Environment", DataType = SqlDbType.NVarChar, DataLength = 50 },
    new SqlColumn { ColumnName = "RequestPath", PropertyName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 250 },
    new SqlColumn { ColumnName = "RequestMethod", PropertyName = "RequestMethod", DataType = SqlDbType.NVarChar, DataLength = 10 },
    new SqlColumn { ColumnName = "StatusCode", PropertyName = "StatusCode", DataType = SqlDbType.Int },
    new SqlColumn { ColumnName = "Elapsed", PropertyName = "Elapsed", DataType = SqlDbType.Int },
    new SqlColumn { ColumnName = "UserName", PropertyName = "UserName", DataType = SqlDbType.NVarChar, DataLength = 100 }
};
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.WithProperty("Application", "Fringe")
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            AutoCreateSqlTable = true,
            BatchPostingLimit = 50,
            BatchPeriod = TimeSpan.FromSeconds(5)
        },
        columnOptions: columnOptions)
);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger with more detailed configuration
app.UseSwagger(c => { c.SerializeAsV2 = false; });

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fringe API v1");
    c.RoutePrefix = string.Empty; // To serve Swagger UI at root
    c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
});

// Enable CORS
app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Add cache control headers for better performance
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
        
        // Enable CORS for static files
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        
        // Log static file requests in development
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"Static file requested: {ctx.File.PhysicalPath}");
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Print the URLs the application is running on
app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls;
    foreach (var url in urls)
    {
        Console.WriteLine($"Application listening on: {url}");
    }
});

app.Run();


