namespace Webshop.DAL.EF
{
    public partial class Telephely
    {
        public int Id { get; set; }
        public string Ir { get; set; }
        public string Varos { get; set; }
        public string Utca { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public int? VevoId { get; set; }

        public Vevo Vevo { get; set; }
    }
}
