using System;
using System.Collections.Generic;

namespace Webshop.DAL.EF
{
    public partial class Vevo
    {
        public Vevo()
        {
            Telephely = new HashSet<Telephely>();
        }

        public int Id { get; set; }
        public string Nev { get; set; }
        public string Szamlaszam { get; set; }
        public string Login { get; set; }
        public string Jelszo { get; set; }
        public string Email { get; set; }
        public int? KozpontiTelephely { get; set; }

        public Telephely KozpontiTelephelyNavigation { get; set; }
        public ICollection<Telephely> Telephely { get; set; }
    }
}
