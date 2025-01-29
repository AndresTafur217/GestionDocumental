using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionArhivos.Data;
using GestionArhivos.Models;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace GestionArhivos.Controllers
{
    public class DocumentoesController : Controller
    {
        private readonly gestionDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DocumentoesController(gestionDBContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Documentoes
        public async Task<IActionResult> Index()
        {
            var gestionDBContext = _context.Documento.Include(d => d.Categoria).Include(d => d.Usuario);
            return View(await gestionDBContext.ToListAsync());
        }

        // GET: Documentoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documento = await _context.Documento
                .Include(d => d.Categoria)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (documento == null)
            {
                return NotFound();
            }

            return View(documento);
        }

        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nombre");
            ViewData["UsuarioId"] = new SelectList(_context.Set<Usuario>(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,UsuarioId,CategoriaId,Ruta")] Documento documento, IFormFile archivo)
        {
            try
            {
                var nuevoDocumento = new Documento
                {
                    Nombre = documento.Nombre,
                    UsuarioId = documento.UsuarioId,
                    CategoriaId = documento.CategoriaId,
                    Ruta = documento.Ruta, // Ubicación física en el cuarto de archivos
                    FechaCreacion = DateTime.Now
                };

                if (archivo != null && archivo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "copias_digitales");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + archivo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    nuevoDocumento.Copia = "/copias_digitales/" + uniqueFileName; // Ruta de la copia digital
                }

                _context.Documento.Add(nuevoDocumento);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear el documento: {ex.Message}");
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nombre", documento.CategoriaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Nombre", documento.UsuarioId);
            return View(documento);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documento = await _context.Documento.FindAsync(id);
            if (documento == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nombre", documento.CategoriaId);
            ViewData["UsuarioId"] = new SelectList(_context.Set<Usuario>(), "Id", "Nombre", documento.UsuarioId);
            return View(documento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,UsuarioId,CategoriaId,Ruta,Copia,FechaCreacion")] Documento documento, IFormFile archivo)
        {
            if (id != documento.Id)
            {
                return NotFound();
            }

            try
            {
                var documentoExistente = await _context.Documento.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                if (documentoExistente == null)
                {
                    return NotFound();
                }

                documento.FechaCreacion = documentoExistente.FechaCreacion;

                if (archivo != null && archivo.Length > 0)
                {
                    // Eliminar archivo anterior si existe
                    if (!string.IsNullOrEmpty(documentoExistente.Copia))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, documentoExistente.Copia.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Guardar nuevo archivo
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + archivo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    documento.Copia = "/copias_digitales/" + uniqueFileName;
                }
                else
                {
                    documento.Copia = documentoExistente.Copia;
                }

                _context.Update(documento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException && !DocumentoExists(documento.Id))
                {
                    return NotFound();
                }

                ModelState.AddModelError("", $"Error al actualizar el documento: {ex.Message}");
            }

            ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nombre", documento.CategoriaId);
            ViewData["UsuarioId"] = new SelectList(_context.Set<Usuario>(), "Id", "Nombre", documento.UsuarioId);
            return View(documento);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var documento = await _context.Documento.FindAsync(id);
            if (documento == null || string.IsNullOrEmpty(documento.Copia))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, documento.Copia.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, contentType, fileName);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documento = await _context.Documento
                .Include(d => d.Categoria)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (documento == null)
            {
                return NotFound();
            }

            return View(documento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var documento = await _context.Documento.FindAsync(id);
            if (documento != null)
            {
                // Eliminar el archivo físico si existe
                if (!string.IsNullOrEmpty(documento.Copia))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, documento.Copia.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Documento.Remove(documento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DocumentoExists(int id)
        {
            return _context.Documento.Any(e => e.Id == id);
        }
    }
}