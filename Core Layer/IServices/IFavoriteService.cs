using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.FavoriteDtos;

namespace Core_Layer.IServices
{
    public interface IFavoriteService
    {
        Task<bool> TToggleFavoriteAsync(Guid productId);
        Task<List<WishlistDto>> TGetWishListAsync();
    }
}
