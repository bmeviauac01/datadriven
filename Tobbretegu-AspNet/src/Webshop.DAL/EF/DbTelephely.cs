using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webshop.DAL.EF
{
    [Table("Telephely")]
    internal partial class DbTelephely
    {
        [Key]
        public int ID { get; set; }

        [StringLength(4)]
        public string IR { get; set; }

        [StringLength(50)]
        public string Varos { get; set; }

        [StringLength(50)]
        public string Utca { get; set; }

        [StringLength(15)]
        public string Tel { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        public int? VevoID { get; set; }

        public virtual DbVevo Vevo { get; set; }
    }
}
