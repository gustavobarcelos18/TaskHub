using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tarefa>(entity =>
        {
            entity.ToTable("TAREFAS");

            entity.HasKey(tarefa => tarefa.Id);

            entity.Property(tarefa => tarefa.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();

            entity.Property(tarefa => tarefa.Descricao)
                .HasColumnName("DESCRICAO")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(tarefa => tarefa.Situacao)
                .HasColumnName("SITUACAO")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(tarefa => tarefa.CriadaEm)
                .HasColumnName("CRIADA_EM")
                .IsRequired();

            entity.Property(tarefa => tarefa.ModificadaEm)
                .HasColumnName("MODIFICADA_EM");

            entity.Property(tarefa => tarefa.SituacaoAlteradaEm)
                .HasColumnName("SITUACAO_ALTERADA_EM")
                .IsRequired();

            entity.Property(tarefa => tarefa.ConcluidaEm)
                .HasColumnName("CONCLUIDA_EM");

            entity.Property(tarefa => tarefa.ExcluidaEm)
                .HasColumnName("EXCLUIDA_EM");

            entity.HasQueryFilter(
                tarefa => tarefa.ExcluidaEm == null
            );
        });
    }
}