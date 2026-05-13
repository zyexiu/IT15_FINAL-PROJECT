using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// ── Database ─────────────────────────────────────────────────
var useRenderDatabase = string.Equals(Environment.GetEnvironmentVariable("RENDER_USE_SQLITE"), "true", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useRenderDatabase)
    {
        var sqlitePath = Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? "snackflow_render.db";
        options.UseSqlite($"Data Source={sqlitePath}");
        return;
    }

    var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    options.UseMySql(
        connStr,
        ServerVersion.AutoDetect(connStr),
        mysql =>
        {
            mysql.EnableRetryOnFailure(maxRetryCount: 3);
            // MySQL has no schema concept — silently ignore any schema annotations
            mysql.SchemaBehavior(MySqlSchemaBehavior.Ignore);
        }
    );
});

// ── ASP.NET Core Identity ────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength         = 8;

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie / session ─────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.AccessDeniedPath  = "/Account/AccessDenied";
    options.ExpireTimeSpan    = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── Email Service ────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();

// ── reCAPTCHA Service ────────────────────────────────────────
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();

// ── Role Menu Service ────────────────────────────────────────
builder.Services.AddScoped<RoleMenuService>();

// ── Notification Service ─────────────────────────────────────
builder.Services.AddScoped<NotificationService>();

// ── MVC ──────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // Add global role-based access control filter
    options.Filters.Add<SnackFlowMES.Filters.RoleAccessFilter>();
});

var app = builder.Build();

// ── Seed on startup ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider;
    try
    {
        var db          = svc.GetRequiredService<ApplicationDbContext>();
        var userManager = svc.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = svc.GetRequiredService<RoleManager<IdentityRole>>();
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db, userManager, roleManager);
    }
    catch (Exception ex)
    {
        svc.GetRequiredService<ILogger<Program>>()
           .LogError(ex, "Database seeding failed.");
    }
}

// ── HTTP pipeline ────────────────────────────────────────────
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
