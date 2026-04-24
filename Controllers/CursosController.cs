using Microsoft.AspNetCore.Mvc;
using PortalAcademico.Data;
using PortalAcademico.Models;
using Microsoft.EntityFrameworkCore;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string nombre, int? minCreditos, int? maxCreditos)
    {
        var cursos = _context.Cursos.AsQueryable();

        if (!string.IsNullOrEmpty(nombre))
            cursos = cursos.Where(c => c.Nombre.Contains(nombre));

        if (minCreditos.HasValue)
            cursos = cursos.Where(c => c.Creditos >= minCreditos);

        if (maxCreditos.HasValue)
            cursos = cursos.Where(c => c.Creditos <= maxCreditos);

        return View(cursos.ToList());
    }

    public IActionResult Detalle(int id)
    {
        var curso = _context.Cursos.FirstOrDefault(c => c.Id == id);
        return View(curso);
    }

    public IActionResult MisCursos()
    {
        var cursos = _context.Cursos.ToList();
        return View(cursos);
    }
}