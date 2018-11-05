using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webshop.BL;

namespace Webshop.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Vevo
        public ActionResult Index()
        {
            var vm = new VevoManager();
            return View(vm.ListVevok());
        }
    }
}