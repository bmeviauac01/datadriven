using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Webshop.DAL
{
    public class MegrendelesRepository : IMegrendelesRepository
    {
        public Task<IEnumerable<object>> ListVevoMegrendelesei(int vevoId) => Task.FromResult(Enumerable.Empty<object>());
    }
}
