using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionArhivos.Models
{
    public class Permisos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int DocumentoId { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public bool Lectura { get; set; }

        [Required]
        public bool Escritura { get; set; }

        [Required]
        public bool Eliminacion { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("DocumentoId")]
        public virtual Documento Documento { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }
    }
}