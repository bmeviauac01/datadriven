using System.Collections.Generic;
using System.Threading.Tasks;

namespace Webshop.DAL
{
    public interface IVevoRepository
    {
        Task<IEnumerable<Vevo>> ListVevok(string vevoNev = null);
        Task<Vevo> GetVevoOrNull(int vevoId);
        Task<IEnumerable<VevoKozpontiTelephellyel>> ListVevokTelephellyel(string vevoNev = null);
        Task DeleteVevo(int vevoID);
    }
}
