    using System.Data.Entity;
namespace Webshop.DAL.EF
{
    internal partial class WebshopDb : DbContext
    {
        public WebshopDb(string connectionString)
            : base(connectionString)
        { }

        public virtual DbSet<DbTelephely> Telephelyek { get; set; }
        public virtual DbSet<DbVevo> Vevok { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbTelephely>()
                .Property(e => e.IR)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<DbTelephely>()
                .Property(e => e.Tel)
                .IsUnicode(false);

            modelBuilder.Entity<DbTelephely>()
                .Property(e => e.Fax)
                .IsUnicode(false);

            modelBuilder.Entity<DbVevo>()
                .Property(e => e.Szamlaszam)
                .IsUnicode(false);

            modelBuilder.Entity<DbVevo>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<DbVevo>()
                .HasMany(e => e.Telephelyek)
                .WithOptional(e => e.Vevo)
                .HasForeignKey(e => e.VevoID);
        }
    }
}