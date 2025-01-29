namespace GestionArhivos.Models
{
    public class DocumentoSearchViewModel
    {
        public string NombreDocumento { get; set; }
        public string NombreUsuario { get; set; }
        public int? CategoriaId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public List<Documento> Resultados { get; set; }
        public List<Categoria> Categorias { get; set; }
    }
}
