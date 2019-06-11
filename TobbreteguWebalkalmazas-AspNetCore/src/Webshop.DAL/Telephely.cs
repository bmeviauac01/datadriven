namespace Webshop.DAL
{
    // sajat entitas a Telephelyhez
    // immutable entitas
    public class Telephely
    {
        public readonly string Varos;
        public readonly string Utca;

        public Telephely(string varos, string utca)
        {
            Varos = varos;
            Utca = utca;
        }
    }
}
