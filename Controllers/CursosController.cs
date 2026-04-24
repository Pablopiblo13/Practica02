using Microsoft.AspNetCore.Mvc;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        // =========================
        // LISTADO + FILTROS
        // =========================
        public IActionResult Index(string nombre, int? minCreditos, int? maxCreditos)
        {
            var cursos = _context.Cursos.Where(c => c.Activo);

            if (!string.IsNullOrEmpty(nombre))
                cursos = cursos.Where(c => c.Nombre.Contains(nombre));

            if (minCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos >= minCreditos.Value);

            if (maxCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos <= maxCreditos.Value);

            return View(cursos.ToList());
        }

        // =========================
        // DETALLE
        // =========================
        public IActionResult Detalle(int id)
        {
            var curso = _context.Cursos.FirstOrDefault(c => c.Id == id);

            if (curso == null)
                return NotFound();

            return View(curso);
        }

        // =========================
        // INSCRIPCIÓN (PREGUNTA 3)
        // =========================
        [Authorize]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var curso = _context.Cursos.FirstOrDefault(c => c.Id == id && c.Activo);

            if (curso == null)
                return NotFound();

            // 1. ya inscrito
            var yaExiste = _context.Matriculas
                .Any(m => m.CursoId == id && m.UsuarioId == user.Id);

            if (yaExiste)
            {
                TempData["Mensaje"] = "Ya estás matriculado en este curso";
                return RedirectToAction("Index");
            }

            // 2. cupo máximo
            var inscritos = _context.Matriculas.Count(m => m.CursoId == id);

            if (inscritos >= curso.CupoMaximo)
            {
                TempData["Mensaje"] = "No hay cupos disponibles";
                return RedirectToAction("Index");
            }

            // 3. choque de horario
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

            // 4. crear matrícula en estado PENDIENTE
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

        // =========================
        // MIS CURSOS
        // =========================
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