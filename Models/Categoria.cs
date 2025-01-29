namespace GestionArhivos.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public virtual ICollection<Documento> Documentos { get; set; }
        public virtual ICollection<Permisos> Permisos { get; set; }
    }
}
