using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Webshop.BL;
using Webshop.DAL;

namespace Webshop.Web.Controllers
{
    // REST api
    public class VevoController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            var vm = new VevoManager();
            return Json(vm.ListVevok());
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            var vm = new VevoManager();
            var vevo = vm.GetVevoOrNull(id);
            if (vevo == null)
                return NotFound();
            else
                return Json(vevo);
        }

        // DELETE api/vevo/5
        [HttpDelete]
        [Route("api/vevo/{vevoId}")]
        public IHttpActionResult Delete(int vevoId)
        {
            var vm = new VevoManager();
            var v = vm.GetVevoOrNull(vevoId);
            if (v == null)
                return NotFound();
            else if (vm.TryTorolVevo(vevoId))
                return Ok();
            else
                return base.Conflict();
        }
    }
}