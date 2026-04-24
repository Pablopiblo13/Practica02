using Microsoft.AspNetCore.Mvc;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PortalAcademico;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CursosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(string nombre, int? minCreditos, int? maxCreditos)
{
    string cacheKey = "cursos_list";

    List<Curso>? cursos = HttpContext.Session.GetObject<List<Curso>>(cacheKey);

    var cacheTime = HttpContext.Session.GetString("cursos_time");

    bool cacheExpirado = cacheTime == null ||
        (DateTime.Now - DateTime.Parse(cacheTime)).TotalSeconds > 60;

    if (cursos == null || cacheExpirado)
    {
        cursos = _context.Cursos.Where(c => c.Activo).ToList();

        HttpContext.Session.SetObject(cacheKey, cursos);
        HttpContext.Session.SetString("cursos_time", DateTime.Now.ToString());
    }

    var query = cursos.AsQueryable();

    if (!string.IsNullOrEmpty(nombre))
        query = query.Where(c => c.Nombre.Contains(nombre));

    if (minCreditos.HasValue)
        query = query.Where(c => c.Creditos >= minCreditos.Value);

    if (maxCreditos.HasValue)
        query = query.Where(c => c.Creditos <= maxCreditos.Value);

    return View(query.ToList());
}

        public IActionResult Detalle(int id)
        {
            var curso = _context.Cursos.FirstOrDefault(c => c.Id == id);

            if (curso == null)
                return NotFound();

            HttpContext.Session.SetString("UltimoCurso", curso.Nombre);

            return View(curso);
        }

        [Authorize]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var curso = _context.Cursos.FirstOrDefault(c => c.Id == id && c.Activo);

            if (curso == null)
                return NotFound();

            var yaExiste = _context.Matriculas
                .Any(m => m.CursoId == id && m.UsuarioId == user.Id);

            if (yaExiste)
            {
                TempData["Mensaje"] = "Ya estás matriculado en este curso";
                return RedirectToAction("Index");
            }

            var inscritos = _context.Matriculas.Count(m => m.CursoId == id);

            if (inscritos >= curso.CupoMaximo)
            {
                TempData["Mensaje"] = "No hay cupos disponibles";
                return RedirectToAction("Index");
            }

            var choqueHorario = _context.Matriculas
                .Where(m => m.UsuarioId == user.Id)
                .Include(m => m.Curso)
                .Any(m =>
                    m.Curso.HorarioInicio < curso.HorarioFin &&
                    curso.HorarioInicio < m.Curso.HorarioFin
                );

            if (choqueHorario)
            {
                TempData["Mensaje"] = "Conflicto de horario con otro curso";
                return RedirectToAction("Index");
            }

            var matricula = new Matricula
            {
                CursoId = id,
                UsuarioId = user.Id,
                FechaRegistro = DateTime.Now,
                Estado = "Pendiente"
            };

            _context.Matriculas.Add(matricula);
            _context.SaveChanges();

            TempData["Mensaje"] = "Matrícula registrada en estado Pendiente";

            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> MisCursos()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var cursos = _context.Matriculas
                .Where(m => m.UsuarioId == user.Id)
                .Include(m => m.Curso)
                .Select(m => m.Curso)
                .ToList();

            return View(cursos);
        }
    }
}