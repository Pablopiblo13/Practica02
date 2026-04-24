using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso
            {
                Codigo = "CS101",
                Nombre = "Programación I",
                Creditos = 3,
                CupoMaximo = 30,
                HorarioInicio = DateTime.Now,
                HorarioFin = DateTime.Now.AddHours(2),
                Activo = true
            },
            new Curso
            {
                Codigo = "CS102",
                Nombre = "Base de Datos",
                Creditos = 4,
                CupoMaximo = 25,
                HorarioInicio = DateTime.Now.AddHours(3),
                HorarioFin = DateTime.Now.AddHours(5),
                Activo = true
            },
            new Curso
            {
                Codigo = "CS103",
                Nombre = "Redes",
                Creditos = 2,
                CupoMaximo = 20,
                HorarioInicio = DateTime.Now.AddHours(6),
                HorarioFin = DateTime.Now.AddHours(8),
                Activo = true
            }
        );

        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ===== SEED DE ROLES (CORRECTO) =====
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string roleName = "Coordinador";

    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
    {
        roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
    }

    var email = "coordinador@uni.com";
    var user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

    if (user != null && !userManager.IsInRoleAsync(user, roleName).GetAwaiter().GetResult())
    {
        userManager.AddToRoleAsync(user, roleName).GetAwaiter().GetResult();
    }
}

app.Run();