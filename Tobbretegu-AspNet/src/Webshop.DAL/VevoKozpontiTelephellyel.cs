namespace Webshop.DAL
{
    // sajat entitas a Vevo a kozponti telephelyevel
    public class VevoKozpontiTelephellyel : Vevo
    {
        public readonly Telephely KozpontiTelephely;

        public VevoKozpontiTelephellyel(string nev, string email, Telephely kozpontiTelephely)
            : base(nev, email)
        {
            KozpontiTelephely = kozpontiTelephely;
        }

    }
}
