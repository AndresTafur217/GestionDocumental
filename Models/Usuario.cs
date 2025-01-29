using System.ComponentModel.DataAnnotations;

namespace GestionArhivos.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(70)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [StringLength(70)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El cargo es obligatorio")]
        [StringLength(40)]
        public string Cargo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255)]
        public string Contraseña { get; set; }

        public virtual ICollection<Documento> Documentos { get; set; }
        public virtual ICollection<Permisos> Permisos { get; set; }
    }
}
