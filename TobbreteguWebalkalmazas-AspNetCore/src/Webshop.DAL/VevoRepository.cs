using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webshop.DAL.EF;

namespace Webshop.DAL
{
    // repository minta a Vevo es kapcsolodo funkciokhoy
    public class VevoRepository : IVevoRepository
    {
        private readonly WebshopDb db;

        public VevoRepository(WebshopDb db) => this.db = db;

        public async Task<Vevo> GetVevoOrNull(int vevoId)
        {
            var dbVevo = await db.Vevo
                                 .GetByIdOrNull(vevoId);
            return dbVevo?.GetVevo();
            // ?. null propagating operator, ha nincs ilyen vevo, a GetById null-t ad vissza
            // a tovabbi fuggvenyhivas hibat dobna null objektumon, a ?. eseten ha null a
            // bal oldal, az eredmeny is null
        }

        // parameter default ertek, nem kell megadni, ha nem akarunk nevre keresni
        public async Task<IEnumerable<Vevo>> ListVevok(string vevoNev = null)
        {
            return await db.Vevo
                           .SearchByName(vevoNev)
                           .GetVevok();
        }

        public async Task<IEnumerable<VevoKozpontiTelephellyel>> ListVevokTelephellyel(string vevoNev = null)
        {
            // "Fluent API" jellegu lekerdezes, a technikai reszletek seged fuggvenyekben
            return await db.Vevo
                           .Telephellyel()
                           .SearchByName(vevoNev)
                           .GetVevokKozpontiTelephellyel();
        }

        public async Task DeleteVevo(int vevoID)
        {
            int retries = 3;
            while (true) // konkurenciakezeles miatt ujraprobalas, de nem vegtelenszer
            {
                var vevoRec = await db.Vevo
                                      .Telephellyel()
                                      .GetByIdOrNull(vevoID);
                if (vevoRec == null) // mar torolve (konkurrens muvelet)
                    return;

                db.Vevo.Remove(vevoRec);

                try
                {
                    await db.SaveChangesAsync();
                    return; // sikeres torles, while megszakitasa
                }
                catch (DbUpdateConcurrencyException ex) // csak optimista konkurenciakezeles hiba, nem barmilyen exception!
                {
                    if (--retries < 0) // vegtelen ciklus elkerulese, ha parszor nem sikerul
                        throw;

                    // frissitsuk be az adatbazisbol es probaljuk ujra
                    foreach (var e in ex.Entries)
                        await e.ReloadAsync();
                }
            }
        }
    }

    // seged fuggvenyek a vevokkel kapcsolatos Linq 2 Entities lekerdezesekhez
    internal static class VevoExtensions
    {
        public static async Task<IEnumerable<Vevo>> GetVevok(this IQueryable<EF.Vevo> vevok)
        {
            return await vevok.Select(dbVevo => dbVevo.GetVevo())
                              .ToArrayAsync();
            // bar IQueryable a fuggveny visszateresi erteke, a ToArrayAsync() segitsegevel kiertekeltetjuk
            // a lekerdezest (ha nem tennenk, hibara furnank, mert a DbContext mar bezarodna mire
            // tenyleg kiertekelesre kerul)
        }

        public static Vevo GetVevo(this EF.Vevo dbVevo) => new Vevo(dbVevo.Nev, dbVevo.Email);

        public static IQueryable<EF.Vevo> SearchByName(this IQueryable<EF.Vevo> vevok, string keresettNev)
        {
            // ha nincs keresett nev, null string jelzi, a teljes "tablat" adjuk vissza
            if (string.IsNullOrEmpty(keresettNev))
                return vevok;
            else // szures a keresett nevre
                return vevok.Where(v => v.Nev.Contains(keresettNev));
        }


        public static IQueryable<EF.Vevo> Telephellyel(this IQueryable<EF.Vevo> vevok) => vevok.Include(v => v.Telephelyek);

        public static async Task<IEnumerable<VevoKozpontiTelephellyel>> GetVevokKozpontiTelephellyel(this IQueryable<EF.Vevo> vevok)
        {
            return await vevok.Select(dbVevo => GetVevoKozpontiTelephellyel(dbVevo))
                              .ToArrayAsync();
        }

        public static Telephely GetTelephely(EF.Telephely dbTelephely) => new Telephely(dbTelephely.Varos, dbTelephely.Utca);

        public static VevoKozpontiTelephellyel GetVevoKozpontiTelephellyel(EF.Vevo dbVevo)
            => new VevoKozpontiTelephellyel(dbVevo.Nev, dbVevo.Email, GetTelephely(dbVevo.KozpontiTelephely));

        public static Task<EF.Vevo> GetByIdOrNull(this IQueryable<EF.Vevo> vevok, int vevoId) => vevok.FirstOrDefaultAsync(v => v.Id == vevoId);
    }
}
