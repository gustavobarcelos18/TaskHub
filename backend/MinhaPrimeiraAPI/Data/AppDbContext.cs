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

    public DbSet<HistoricoTarefa> HistoricosTarefas => Set<HistoricoTarefa>();

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

            entity.Property(tarefa => tarefa.Prioridade)
                .HasColumnName("PRIORIDADE")
                .HasMaxLength(10)
                .HasDefaultValue(PrioridadesTarefa.Media)
                .IsRequired();

            entity.Property(tarefa => tarefa.DataVencimento)
                .HasColumnName("DATA_VENCIMENTO");

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

        modelBuilder.Entity<HistoricoTarefa>(entity =>
        {
            entity.ToTable("HISTORICO_TAREFAS");

            entity.HasKey(historico => historico.Id);

            entity.Property(historico => historico.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();

            entity.Property(historico => historico.TarefaId)
                .HasColumnName("TAREFA_ID")
                .IsRequired();

            entity.Property(historico => historico.Tipo)
                .HasColumnName("TIPO")
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(historico => historico.Campo)
                .HasColumnName("CAMPO")
                .HasMaxLength(50);

            entity.Property(historico => historico.ValorAnterior)
                .HasColumnName("VALOR_ANTERIOR")
                .HasMaxLength(200);

            entity.Property(historico => historico.ValorNovo)
                .HasColumnName("VALOR_NOVO")
                .HasMaxLength(200);

            entity.Property(historico => historico.CriadoEm)
                .HasColumnName("CRIADO_EM")
                .IsRequired();

            entity.HasIndex(historico => new { historico.TarefaId, historico.CriadoEm });

            entity.HasOne(historico => historico.Tarefa)
                .WithMany()
                .HasForeignKey(historico => historico.TarefaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(historico => historico.Tarefa.ExcluidaEm == null);
        });
    }
}
