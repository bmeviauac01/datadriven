namespace Webshop.DAL
{
    /// <summary>
    /// Az adatbazis connection string-et adja meg.
    /// Az app.config helyett barhonnan (pl. sajat config fajl) beolvashatjuk a connstringet.
    /// </summary>
    public static class ConnectionStringHelper
    {
        public static string ConnectionString;
    }
}
