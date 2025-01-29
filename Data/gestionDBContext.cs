using GestionArhivos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionArhivos.Data;
public class gestionDBContext : DbContext
{
    public gestionDBContext(DbContextOptions<gestionDBContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    public DbSet<Documento> Documento { get; set; }
    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<Categoria> Categoria { get; set; }
    public DbSet<Permisos> Permiso { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Correo)
            .IsUnique();
    }
}