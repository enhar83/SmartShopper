using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.IServices
{
    public interface IFavoriteService
    {
        Task<bool> TTogleFavoriteAsync(Guid productId);
    }
}
