using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Crear rol Coordinador
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
    }

    // Crear usuario coordinador
    var email = "admin@demo.com";
    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        user = new IdentityUser { UserName = email, Email = email };
        await userManager.CreateAsync(user, "Admin123!");
        await userManager.AddToRoleAsync(user, "Coordinador");
    }

    // Crear cursos iniciales
    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso {
                Codigo = "CS101",
                Nombre = "Programación I",
                Creditos = 3,
                CupoMaximo = 30,
                HorarioInicio = DateTime.Now,
                HorarioFin = DateTime.Now.AddHours(2),
                Activo = true
            },
            new Curso {
                Codigo = "CS102",
                Nombre = "Base de Datos",
                Creditos = 4,
                CupoMaximo = 25,
                HorarioInicio = DateTime.Now.AddHours(3),
                HorarioFin = DateTime.Now.AddHours(5),
                Activo = true
            },
            new Curso {
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
