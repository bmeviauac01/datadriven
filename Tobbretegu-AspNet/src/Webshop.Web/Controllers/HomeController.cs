using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Webshop.BL;

namespace Webshop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly VevoManager vm;

        public HomeController(VevoManager vm) => this.vm = vm;

        public async Task<IActionResult> Index() => View(await vm.ListVevok());
    }
}
