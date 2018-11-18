using System.Collections.Generic;
using System.Threading.Tasks;

namespace Webshop.DAL
{
    public interface IMegrendelesRepository
    {
        Task<IEnumerable<object>> ListVevoMegrendelesei(int vevoId);
    }
}
