using Afetto_.Net.Models;

using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Afetto_.Net.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Cidade> Cidades { get; set; }
        public DbSet<Bairro> Bairros { get; set; }
        public DbSet<Logradouro> Logradouros { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pet> Pets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── PAIS ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Pais>(e =>
            {
                e.ToTable("TB_PAIS");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_PAIS").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
                e.Property(x => x.Sigla).HasColumnName("SIGLA").HasMaxLength(3).IsRequired();
            });

            // ── ESTADO ────────────────────────────────────────────────────────
            modelBuilder.Entity<Estado>(e =>
            {
                e.ToTable("TB_ESTADO");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_ESTADO").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
                e.Property(x => x.Sigla).HasColumnName("SIGLA").HasMaxLength(2).IsRequired();
                e.Property(x => x.PaisId).HasColumnName("ID_PAIS").HasColumnType("RAW(16)");

                e.HasOne(x => x.Pais)
                 .WithMany(p => p.Estados)
                 .HasForeignKey(x => x.PaisId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── CIDADE ────────────────────────────────────────────────────────
            modelBuilder.Entity<Cidade>(e =>
            {
                e.ToTable("TB_CIDADE");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_CIDADE").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
                e.Property(x => x.EstadoId).HasColumnName("ID_ESTADO").HasColumnType("RAW(16)");

                e.HasOne(x => x.Estado)
                 .WithMany(s => s.Cidades)
                 .HasForeignKey(x => x.EstadoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── BAIRRO ────────────────────────────────────────────────────────
            modelBuilder.Entity<Bairro>(e =>
            {
                e.ToTable("TB_BAIRRO");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_BAIRRO").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(150).IsRequired();
                e.Property(x => x.CidadeId).HasColumnName("ID_CIDADE").HasColumnType("RAW(16)");

                e.HasOne(x => x.Cidade)
                 .WithMany(c => c.Bairros)
                 .HasForeignKey(x => x.CidadeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── LOGRADOURO ────────────────────────────────────────────────────
            modelBuilder.Entity<Logradouro>(e =>
            {
                e.ToTable("TB_LOGRADOURO");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_LOGRADOURO").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(200).IsRequired();
                e.Property(x => x.Cep).HasColumnName("CEP").HasMaxLength(8).IsRequired();
                e.Property(x => x.Tipo).HasColumnName("TIPO").HasMaxLength(50);
                e.Property(x => x.BairroId).HasColumnName("ID_BAIRRO").HasColumnType("RAW(16)");

                e.HasOne(x => x.Bairro)
                 .WithMany(b => b.Logradouros)
                 .HasForeignKey(x => x.BairroId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── USUARIO ───────────────────────────────────────────────────────
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("TB_USUARIO");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_USUARIO").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(150).IsRequired();
                e.Property(x => x.Cpf).HasColumnName("CPF").HasMaxLength(11).IsRequired();
                e.Property(x => x.DataNasc).HasColumnName("DATA_NASC").HasColumnType("DATE");
                e.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(200).IsRequired();
                e.Property(x => x.Senha).HasColumnName("SENHA").HasMaxLength(255).IsRequired();
                e.Property(x => x.Telefone).HasColumnName("TELEFONE").HasMaxLength(15);
                e.Property(x => x.Numero).HasColumnName("NUMERO").HasMaxLength(10);
                e.Property(x => x.Complemento).HasColumnName("COMPLEMENTO").HasMaxLength(100);
                e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").HasColumnType("TIMESTAMP");
                e.Property(x => x.LogradouroId).HasColumnName("ID_LOGRADOURO").HasColumnType("RAW(16)");

                e.HasIndex(x => x.Cpf).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();

                e.HasOne(x => x.Logradouro)
                 .WithMany(l => l.Usuarios)
                 .HasForeignKey(x => x.LogradouroId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── PET ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Pet>(e =>
            {
                e.ToTable("TB_PET");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID_PET").HasColumnType("RAW(16)");
                e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
                e.Property(x => x.Especie).HasColumnName("ESPECIE").HasMaxLength(50).IsRequired();
                e.Property(x => x.Raca).HasColumnName("RACA").HasMaxLength(100);
                e.Property(x => x.Sexo).HasColumnName("SEXO").HasMaxLength(1);
                e.Property(x => x.Peso).HasColumnName("PESO");
                e.Property(x => x.DataNasc).HasColumnName("DATA_NASC").HasColumnType("DATE");
                e.Property(x => x.Descricao).HasColumnName("DESCRICAO").HasColumnType("CLOB");
                e.Property(x => x.QrCodeToken).HasColumnName("QR_CODE_TOKEN").HasMaxLength(36).IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").HasColumnType("TIMESTAMP");
                e.Property(x => x.UsuarioId).HasColumnName("ID_USUARIO").HasColumnType("RAW(16)");

                e.HasIndex(x => x.QrCodeToken).IsUnique();

                e.HasOne(x => x.Usuario)
                 .WithMany(u => u.Pets)
                 .HasForeignKey(x => x.UsuarioId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}