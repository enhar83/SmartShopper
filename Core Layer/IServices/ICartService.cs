using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CartDtos;

namespace Core_Layer.IServices
{
    public interface ICartService
    {
        Task<CartDto> TGetUserCartAsync(Guid userId);
        Task TAddToCartAsync(Guid userId, CreateCartItemDto createCartItemDto);
        Task TUpdateCartItemAsync(UpdateCartItemDto updateCartItemDto);
    }
}
