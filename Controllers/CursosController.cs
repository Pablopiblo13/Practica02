using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoordinadorController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var cursos = await _context.Cursos.ToListAsync();
        return View(cursos);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Curso curso)
    {
        if (ModelState.IsValid)
        {
            _context.Add(curso);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);

        if (curso == null)
            return NotFound();

        return View(curso);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Curso curso)
    {
        _context.Update(curso);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Toggle(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);

        if (curso == null)
            return NotFound();

        curso.Activo = !curso.Activo;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Matriculas(int id)
    {
        var data = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.CursoId == id)
            .ToListAsync();

        return View(data);
    }

    public async Task<IActionResult> Confirmar(int id)
    {
        var m = await _context.Matriculas.FindAsync(id);

        if (m == null)
            return NotFound();

        m.Estado = "Confirmada";

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Cancelar(int id)
    {
        var m = await _context.Matriculas.FindAsync(id);

        if (m == null)
            return NotFound();

        m.Estado = "Cancelada";

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}