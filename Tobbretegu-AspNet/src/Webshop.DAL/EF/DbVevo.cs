using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webshop.DAL.EF
{
    [Table("Vevo")]
    internal partial class DbVevo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DbVevo()
        {
            Telephelyek = new HashSet<DbTelephely>();
        }

        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        public string Nev { get; set; }

        [StringLength(50)]
        public string Szamlaszam { get; set; }

        [StringLength(50)]
        public string Login { get; set; }

        [StringLength(50)]
        public string Jelszo { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [Column("KozpontiTelephely")]
        [ForeignKey(nameof(KozpontiTelephely))]
        public int? KozpontiTelephelyID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DbTelephely> Telephelyek { get; set; }

        public virtual DbTelephely KozpontiTelephely { get; set; }
    }
}
