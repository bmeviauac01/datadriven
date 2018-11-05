namespace Webshop.DAL
{
    // sajat entitas a Vevo reprezentalasahoz
    // immutable entitas
    public class Vevo
    {
        public readonly string Nev;
        public readonly string Email;

        public Vevo(string nev, string email)
        {
            Nev = nev;
            Email = email;
        }
    }
}
