using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestionArhivos.Models
{
    public class Documento
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(500)]
        public string Ruta { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Copia { get; set; } = string.Empty;

        [Required]
        public DateTime FechaCreacion { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }
        public virtual ICollection<Permisos> Permisos { get; set; }
    }
}
