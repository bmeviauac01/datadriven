using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Webshop.DAL.EF;

namespace Webshop.DAL
{
    // repository minta a Vevo es kapcsolodo funkciokhoy
    public class VevoRepository : IVevoRepository
    {
        private WebshopDb getDB()
        {
            return new WebshopDb(ConnectionStringHelper.ConnectionString);
        }

        public Vevo GetVevoOrNull(int vevoId)
        {
            using (var db = getDB())
                return db.Vevok
                         .GetByIdOrNull(vevoId)
                         ?.GetVevo();
                         // ?. null propagating operator, ha nincs ilyen vevo, a GetById null-t ad vissza
                         // a tovabbi fuggvenyhivas hibat dobna null objektumon, a ?. eseten ha null a
                         // bal oldal, az eredmeny is null
        }

        // parameter default ertek, nem kell megadni, ha nem akarunk nevre keresni
        public IEnumerable<Vevo> ListVevok(string vevoNev = null)
        {
            using (var db = getDB())
                return db.Vevok
                         .SearchByName(vevoNev)
                         .GetVevok();
        }

        public IEnumerable<VevoKozpontiTelephellyel> ListVevokTelephellyel(string vevoNev = null)
        {
            // "Fluent API" jellegu lekerdezes, a technikai reszletek seged fuggvenyekben
            using (var db = getDB())
                return db.Vevok
                         .Telephellyel()
                         .SearchByName(vevoNev)
                         .GetVevokKozpontiTelephellyel();
        }

        public void DeleteVevo(int vevoID)
        {
            using (var db = getDB())
            {
                int retries = 3;
                while (true) // konkurenciakezeles miatt ujraprobalas, de nem vegtelenszer
                {
                    var vevoRec = db.Vevok.GetByIdOrNull(vevoID);
                    if (vevoRec == null) // mar torolve (konkurrens muvelet)
                        return;

                    db.Telephelyek.RemoveRange(vevoRec.Telephelyek);
                    db.Vevok.Remove(vevoRec);

                    try
                    {
                        db.SaveChanges();
                        return; // sikeres torles, while megszakitasa
                    }
                    catch (DbUpdateConcurrencyException ex) // csak optimista konkurenciakezeles hiba, nem barmilyen exception!
                    {
                        if (--retries < 0) // vegtelen ciklus elkerulese, ha parszor nem sikerul
                            throw;

                        // frissitsuk be az adatbazisbol es probaljuk ujra
                        foreach (var e in ex.Entries)
                            e.Reload();
                    }
                }
            }
        }
    }

    // seged fuggvenyek a vevokkel kapcsolatos Linq 2 Entities lekerdezesekhez
    internal static class VevoExtensions
    {
        public static IEnumerable<Vevo> GetVevok(this IQueryable<DbVevo> vevok)
        {
            return vevok.Select(GetVevo)
                        .ToArray();
            // bar IEnumerable a fuggveny visszateresi erteke, a ToArray() segitsegevel kiertekeltetjuk
            // a lekerdezest (ha nem tennenk, hibara furnank, mert a DbContext mar bezarodna mire
            // tenyleg kiertekelesre kerul)
        }

        public static Vevo GetVevo(this EF.DbVevo dbVevo)
        {
            return new Vevo(dbVevo.Nev, dbVevo.Email);
        }

        public static IQueryable<DbVevo> SearchByName(this IQueryable<DbVevo> vevok, string keresettNev)
        {
            // ha nincs keresett nev, null string jelzi, a teljes "tablat" adjuk vissza
            if (string.IsNullOrEmpty(keresettNev))
                return vevok;
            else // szures a keresett nevre
                return vevok.Where(v => v.Nev.Contains(keresettNev));
        } 


        public static IQueryable<DbVevo> Telephellyel(this IQueryable<DbVevo> vevok)
        {
            return vevok.Include(v => v.Telephelyek);
        }

        public static IEnumerable<VevoKozpontiTelephellyel> GetVevokKozpontiTelephellyel(this IQueryable<DbVevo> vevok)
        {
            return vevok.Select(GetVevoKozpontiTelephellyel)
                        .ToArray();
        }

        public static Telephely GetTelephely(EF.DbTelephely dbTelephely)
        {
            return new Telephely(dbTelephely.Varos, dbTelephely.Utca);
        }

        public static VevoKozpontiTelephellyel GetVevoKozpontiTelephellyel(EF.DbVevo dbVevo)
        {
            return new VevoKozpontiTelephellyel(dbVevo.Nev, dbVevo.Email, GetTelephely(dbVevo.KozpontiTelephely));
        }

        public static DbVevo GetByIdOrNull(this IQueryable<DbVevo> vevok, int vevoId)
        {
            return vevok.FirstOrDefault(v => v.ID == vevoId);
        }
    }
}
