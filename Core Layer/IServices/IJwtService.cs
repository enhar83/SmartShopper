using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer;

namespace Core_Layer.IServices
{
    public interface IJwtService
    {
        Task<string> TCreateToken(AppUser user);
    }
}
