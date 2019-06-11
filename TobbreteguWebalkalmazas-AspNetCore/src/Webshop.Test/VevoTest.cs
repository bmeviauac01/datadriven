using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Webshop.BL;
using Webshop.DAL;

namespace Webshop.Test
{
    [TestClass] // teszt kodot ilyen osztalyba irunk
    public class VevoTest
    {
        [TestMethod] // ez egy teszt
        public async Task TestTorolNemletezoVevo()
        {
            // Unit test: Arrange (elokeszites), Act (beavatkozas), Assert (elvart eredmeny ellenorzese)

            // repository mock-olasa, egy helyettesito objektummal a teszthez
            // ebben a tesztben nem az adatbazist teszteljuk, hanem a VevoManager-t, adatbazis nelkul

            // ez a mock objektum
            var vevoRepo = new Mock<IVevoRepository>();
            // mock beallitasa: ha GetVevo-t hivnak barmilyen parameterrel, a valasz null
            vevoRepo.Setup(repo => repo.GetVevoOrNull(It.IsAny<int>())).ReturnsAsync((Vevo)null);

            var megrendelesRepo = new Mock<IMegrendelesRepository>();
            megrendelesRepo.Setup(repo => repo.ListVevoMegrendelesei(It.IsAny<int>())).ReturnsAsync(Enumerable.Empty<object>());

            // VevoManager peldanyositasa a mock-olt repositoryval
            var vm = new VevoManager(vevoRepo.Object, megrendelesRepo.Object);
            // a TryTorolVevo-nek false kell legyen a visszateresi erteke
            Assert.IsFalse(await vm.TryTorolVevo(123));
        }
    }
}
