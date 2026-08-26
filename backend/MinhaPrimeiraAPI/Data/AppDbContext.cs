using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjetoTarefas.Models;

namespace ProjetoTarefas.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    public DbSet<HistoricoTarefa> HistoricosTarefas => Set<HistoricoTarefa>();

    public DbSet<Etiqueta> Etiquetas => Set<Etiqueta>();

    public DbSet<Projeto> Projetos => Set<Projeto>();

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

            entity.Property(tarefa => tarefa.UsuarioId)
                .HasColumnName("USUARIO_ID");

            entity.HasIndex(tarefa => tarefa.UsuarioId);

            entity.HasOne(tarefa => tarefa.Usuario)
                .WithMany()
                .HasForeignKey(tarefa => tarefa.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(tarefa => tarefa.Descricao)
                .HasColumnName("DESCRICAO")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(tarefa => tarefa.Observacoes)
                .HasColumnName("OBSERVACOES")
                .HasMaxLength(4000);

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

            entity.Property(tarefa => tarefa.ProjetoId)
                .HasColumnName("PROJETO_ID");

            entity.HasIndex(tarefa => tarefa.ProjetoId);

            entity.HasOne(tarefa => tarefa.Projeto)
                .WithMany(projeto => projeto.Tarefas)
                .HasForeignKey(tarefa => tarefa.ProjetoId)
                .OnDelete(DeleteBehavior.SetNull);

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
                .HasMaxLength(4000);

            entity.Property(historico => historico.ValorNovo)
                .HasColumnName("VALOR_NOVO")
                .HasMaxLength(4000);

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

        modelBuilder.Entity<Etiqueta>(entity =>
        {
            entity.ToTable("ETIQUETAS");
            entity.HasKey(etiqueta => etiqueta.Id);
            entity.Property(etiqueta => etiqueta.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(etiqueta => etiqueta.UsuarioId).HasColumnName("USUARIO_ID");
            entity.HasOne(etiqueta => etiqueta.Usuario).WithMany().HasForeignKey(etiqueta => etiqueta.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(etiqueta => etiqueta.Nome).HasColumnName("NOME").HasMaxLength(50).IsRequired();
            entity.Property(etiqueta => etiqueta.NomeNormalizado).HasColumnName("NOME_NORMALIZADO").HasMaxLength(50).IsRequired();
            entity.HasIndex(etiqueta => new { etiqueta.UsuarioId, etiqueta.NomeNormalizado }).IsUnique();
        });

        modelBuilder.Entity<Projeto>(entity =>
        {
            entity.ToTable("PROJETOS");
            entity.HasKey(projeto => projeto.Id);
            entity.Property(projeto => projeto.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(projeto => projeto.UsuarioId).HasColumnName("USUARIO_ID");
            entity.HasOne(projeto => projeto.Usuario).WithMany().HasForeignKey(projeto => projeto.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(projeto => projeto.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
            entity.Property(projeto => projeto.NomeNormalizado).HasColumnName("NOME_NORMALIZADO").HasMaxLength(100).IsRequired();
            entity.HasIndex(projeto => new { projeto.UsuarioId, projeto.NomeNormalizado }).IsUnique();
        });

        modelBuilder.Entity<Tarefa>()
            .HasMany(tarefa => tarefa.Etiquetas)
            .WithMany(etiqueta => etiqueta.Tarefas)
            .UsingEntity<Dictionary<string, object>>(
                "TarefaEtiqueta",
                direita => direita.HasOne<Etiqueta>().WithMany().HasForeignKey("ETIQUETA_ID").OnDelete(DeleteBehavior.Cascade),
                esquerda => esquerda.HasOne<Tarefa>().WithMany().HasForeignKey("TAREFA_ID").OnDelete(DeleteBehavior.Cascade),
                associacao =>
                {
                    associacao.ToTable("TAREFA_ETIQUETA");
                    associacao.HasKey("TAREFA_ID", "ETIQUETA_ID");
                });
    }
}
