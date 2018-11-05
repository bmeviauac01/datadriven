using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Webshop.DAL;

namespace Webshop.BL
{
    // uzleti folyamatok implementalasa
    public class VevoManager
    {
        // tetszoleges repository-val tud mukodni, lasd a tesztelesnel
        // elegansabb lenne: dependency injection, pl. Unity
        private readonly IVevoRepository repo;

        public VevoManager(IVevoRepository repo)
        {
            this.repo = repo;
        }

        // default construktor, EF es adatbazis alapu repository
        public VevoManager()
            : this(new VevoRepository())
        { }

        // visszateresi ertek nem List vagy tomb, az IEnumerable csak olvashato
        // szemantikailag jobban illik egy lekerdezesre, nem modosithato az eredmenye
        public IEnumerable<Vevo> ListVevok(string keresettNev = null)
        {
            return repo.ListVevok(keresettNev);
        }

        public Vevo GetVevoOrNull(int vevoId)
        {
            return repo.GetVevoOrNull(vevoId);
        }

        public bool TryTorolVevo(int vevoId)
        {
            // tranzakcio, hogy ne lehessen uj megrendelest rogziteni kozben
            // tranzakcio nem a DAL szinten, mert itt kell a ket, tranzakcio nelkuli muveletet egyben vegrehajtani
            // (business workflow -> tranzakcio hatar)
            using (var tran = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.RepeatableRead }))
            {
                // letezik-e a vevo?
                // plusz: lekerdezes repetable read-en zarolja a vevo rekordot, nem tudja kozben mas is torolni
                var vevo = repo.GetVevoOrNull(vevoId);
                if (vevo == null)
                    return false;

                // van-e megrendelese?
                // plusz: lekerdezes repetable read-en zarolja a vevo rekordot, nem leeht kozben felvenni uj megrendelest
                bool vanMegrendelese = new MegrendelesManager().ListVevoMegrendelesei(vevoId).Any();
                if (vanMegrendelese)
                    return false;

                // minden ok, torolheto
                repo.DeleteVevo(vevoId);

                // tranzakciot explicit zarni kell, mert mi nyitottuk
                tran.Complete();
                return true;
            }
        }
    }
}
