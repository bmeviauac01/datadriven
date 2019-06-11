using Microsoft.EntityFrameworkCore;

namespace Webshop.DAL.EF
{
    public partial class WebshopDb : DbContext
    {
        public WebshopDb()
        {
        }

        public WebshopDb(DbContextOptions<WebshopDb> options)
            : base(options)
        {
        }

        public virtual DbSet<Telephely> Telephely { get; set; }
        public virtual DbSet<Vevo> Vevo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Telephely>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Fax)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Ir)
                    .HasColumnName("IR")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Utca).HasMaxLength(50);

                entity.Property(e => e.Varos).HasMaxLength(50);

                entity.Property(e => e.VevoId).HasColumnName("VevoID");

                entity.HasOne(d => d.Vevo)
                    .WithMany(p => p.Telephelyek)
                    .HasForeignKey(d => d.VevoId);
            });

            modelBuilder.Entity<Vevo>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Jelszo).HasMaxLength(50);

                entity.Property(e => e.Login).HasMaxLength(50);

                entity.Property(e => e.Nev).HasMaxLength(50);

                entity.Property(e => e.Szamlaszam)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.KozpontiTelephelyId).HasColumnName("KozpontiTelephely");

                entity.HasOne(d => d.KozpontiTelephely)
                    .WithOne()
                    .HasForeignKey<Vevo>(d => d.KozpontiTelephelyId);
            });
        }
    }
}
