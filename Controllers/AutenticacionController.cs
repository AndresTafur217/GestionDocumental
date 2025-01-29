using GestionArhivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using GestionArhivos.Data;

namespace GestionArhivos.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly gestionDBContext _context;
        private const string CLAVE_AUTORIZACION = "ClaveEmpresa2024";

        public AutenticacionController(gestionDBContext context)
        {
            _context = context;
        }

        // Vista de Iniciar Sesión
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contraseña)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null || !VerificarContraseña(contraseña, usuario.Contraseña))
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View();
            }

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

            return RedirectToAction("Index", "Home");
        }

        // Vista de Registro
        public IActionResult Registro()
        {
            return View();
        }

        // Método para verificar la clave de autorización
        [HttpPost]
        public IActionResult VerificarAutorizacion(string claveAutorizacion)
        {
            if (claveAutorizacion == CLAVE_AUTORIZACION)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Clave de autorización incorrecta" });
        }

        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario, string claveAutorizacion)
        {
            if (claveAutorizacion != CLAVE_AUTORIZACION)
            {
                TempData["ErrorMessage"] = "Clave de autorización inválida";
                return View(usuario);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si el correo ya existe
                    if (await _context.Usuario.AnyAsync(u => u.Correo == usuario.Correo))
                    {
                        TempData["ErrorMessage"] = "El correo ya está registrado";
                        return View(usuario);
                    }

                    // Hashear contraseña
                    usuario.Contraseña = HashearContraseña(usuario.Contraseña);

                    _context.Usuario.Add(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario registrado exitosamente";
                    return RedirectToAction(nameof(Login));
                }

                // Si llegamos aquí, hay errores en el ModelState
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    TempData["ErrorMessage"] = modelError.ErrorMessage;
                }
                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al registrar usuario: " + ex.Message;
                return View(usuario);
            }
        }

        // Vista de Recuperar Contraseña
        public IActionResult RecuperarContraseña()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarContraseña(string correo)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado. Verifique el correo electrónico.";
                return View();
            }

            return RedirectToAction(nameof(CambiarContraseña), new { correo });
        }

        // Vista de Cambiar Contraseña
        public IActionResult CambiarContraseña(string correo)
        {
            ViewBag.Email = correo;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContraseña(string correo, string nuevaContraseña)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Error al cambiar la contraseña. Usuario no encontrado.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                usuario.Contraseña = HashearContraseña(nuevaContraseña);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
                return RedirectToAction(nameof(Login));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error al cambiar la contraseña. Intente nuevamente.";
                return RedirectToAction(nameof(Login));
            }
        }

        // Cerrar Sesión
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // Métodos de Utilidad
        private string HashearContraseña(string contraseña)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerificarContraseña(string contraseñaIngresada, string contraseñaAlmacenada)
        {
            return HashearContraseña(contraseñaIngresada) == contraseñaAlmacenada;
        }
    }
}