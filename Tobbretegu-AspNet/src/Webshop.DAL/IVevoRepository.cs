using System.Collections.Generic;

namespace Webshop.DAL
{
    public interface IVevoRepository
    {
        IEnumerable<Vevo> ListVevok(string vevoNev = null);
        Vevo GetVevoOrNull(int vevoId);
        IEnumerable<VevoKozpontiTelephellyel> ListVevokTelephellyel(string vevoNev = null);
        void DeleteVevo(int vevoID);
    }
}