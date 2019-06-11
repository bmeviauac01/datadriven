using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Webshop.DAL;

namespace Webshop.BL
{
    // uzleti folyamatok implementalasa
    public class VevoManager
    {
        // tetszoleges repository-val tud mukodni, lasd a tesztelesnel
        private readonly IVevoRepository vevoRepository;
        private readonly IMegrendelesRepository megrendelesRepository;

        public VevoManager(IVevoRepository vevoRepository, IMegrendelesRepository megrendelesRepository)
        {
            this.vevoRepository = vevoRepository;
            this.megrendelesRepository = megrendelesRepository;
        }

        // visszateresi ertek nem List vagy tomb, az IEnumerable csak olvashato
        // szemantikailag jobban illik egy lekerdezesre, nem modosithato az eredmenye
        public async Task<IEnumerable<Vevo>> ListVevok(string keresettNev = null) => await vevoRepository.ListVevok(keresettNev);

        public async Task<Vevo> GetVevoOrNull(int vevoId) => await vevoRepository.GetVevoOrNull(vevoId);

        public async Task<bool> TryTorolVevo(int vevoId)
        {
            // tranzakcio, hogy ne lehessen uj megrendelest rogziteni kozben
            // tranzakcio nem a DAL szinten, mert itt kell a ket, tranzakcio nelkuli muveletet egyben vegrehajtani
            // (business workflow -> tranzakcio hatar)
            using (var tran = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions() { IsolationLevel = IsolationLevel.RepeatableRead },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // letezik-e a vevo?
                // plusz: lekerdezes repetable read-en zarolja a vevo rekordot, nem tudja kozben mas is torolni
                var vevo = await vevoRepository.GetVevoOrNull(vevoId);
                if (vevo == null)
                    return false;

                // van-e megrendelese?
                // plusz: lekerdezes repetable read-en zarolja a vevo rekordot, nem leeht kozben felvenni uj megrendelest
                bool vanMegrendelese = (await megrendelesRepository.ListVevoMegrendelesei(vevoId)).Any();
                if (vanMegrendelese)
                    return false;

                // minden ok, torolheto
                await vevoRepository.DeleteVevo(vevoId);

                // tranzakciot explicit zarni kell, mert mi nyitottuk
                tran.Complete();
                return true;
            }
        }
    }
}
