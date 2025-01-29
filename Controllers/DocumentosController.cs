using GestionArhivos.Data;
using GestionArhivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class DocumentosController : Controller
{
    private readonly gestionDBContext _context;

    public DocumentosController(gestionDBContext context)
    {
        _context = context;
    }

    // Acción para mostrar el formulario de búsqueda
    public async Task<IActionResult> Buscar()
    {
        var viewModel = new DocumentoSearchViewModel
        {
            Categorias = await _context.Categoria.ToListAsync() // Cargar categorías para el dropdown
        };
        return View(viewModel);
    }

    // Acción para procesar la búsqueda
    [HttpPost]
    public async Task<IActionResult> Buscar(DocumentoSearchViewModel searchModel)
    {
        var query = _context.Documento
            .Include(d => d.Usuario)
            .Include(d => d.Categoria)
            .AsQueryable();

        // Aplicar filtros si se proporcionaron
        if (!string.IsNullOrEmpty(searchModel.NombreDocumento))
        {
            query = query.Where(d => d.Nombre.Contains(searchModel.NombreDocumento));
        }

        if (!string.IsNullOrEmpty(searchModel.NombreUsuario))
        {
            query = query.Where(d => d.Usuario.Nombre.Contains(searchModel.NombreUsuario));
        }

        // Nuevo filtro por categoría
        if (searchModel.CategoriaId.HasValue)
        {
            query = query.Where(d => d.CategoriaId == searchModel.CategoriaId.Value);
        }

        if (searchModel.FechaInicio.HasValue)
        {
            query = query.Where(d => d.FechaCreacion >= searchModel.FechaInicio.Value);
        }

        searchModel.Resultados = await query.ToListAsync();
        searchModel.Categorias = await _context.Categoria.ToListAsync(); // Recargar categorías para mantener el dropdown
        return View(searchModel);
    }
}

