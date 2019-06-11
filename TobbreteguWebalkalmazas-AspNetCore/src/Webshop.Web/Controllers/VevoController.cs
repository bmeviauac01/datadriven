using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webshop.BL;
using Webshop.DAL;

namespace Webshop.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VevoController : ControllerBase
    {
        private readonly VevoManager vm;

        public VevoController(VevoManager vm) => this.vm = vm;

        [HttpGet]
        public async Task<IEnumerable<Vevo>> Get() => await vm.ListVevok();

        [HttpGet("{vevoId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Vevo>> Get(int vevoId)
        {
            var vevo = await vm.GetVevoOrNull(vevoId);
            if (vevo == null)
                return NotFound();
            else
                return Ok(vevo);
        }

        [HttpDelete("{vevoId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Delete(int vevoId)
        {
            var vevo = await vm.GetVevoOrNull(vevoId);
            if (vevo == null)
                return NotFound();
            else if (await vm.TryTorolVevo(vevoId))
                return Ok();
            else
                return Conflict();
        }
    }
}