using System;
using Moq;
using Webshop.BL;
using Webshop.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Webshop.Test
{
    [TestClass] // teszt kodot ilyen osztalyba irunk
    public class VevoTest
    {
        [TestMethod] // ez egy teszt
        public void TestTorolNemletezoVevo()
        {
            // Unit test: Arrange (elokeszites), Act (beavatkozas), Assert (elvart eredmeny ellenorzese)

            // repository mock-olasa, egy helyettesito objektummal a teszthez
            // ebben a tesztben nem az adatbazist teszteljuk, hanem a VevoManager-t, adatbazis nelkul

            // ez a mock objektum
            var vevoRepo = new Mock<IVevoRepository>();
            // mock beallitasa: ha GetVevo-t hivnak barmilyen parameterrel, a valasz null
            vevoRepo.Setup(repo => repo.GetVevoOrNull(It.IsAny<int>())).Returns((Vevo)null);

            // VevoManager peldanyositasa a mock-olt repositoryval
            var vm = new VevoManager(vevoRepo.Object);
            // a TryTorolVevo-nek false kell legyen a visszateresi erteke
            Assert.IsFalse(vm.TryTorolVevo(123));
        }
    }
}
