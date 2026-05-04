using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CartDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class CartManager : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CartManager(ICartRepository cartRepository, IUnitOfWork uow, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<CartDto> TGetUserCartAsync(Guid userId)
        {
            var query = _cartRepository.Where(x => x.AppUserId == userId);

        var cart = await query
            .Include(c => c.CartItems)           
                .ThenInclude(ci => ci.Product)     
                    .ThenInclude(p => p.ProductImages) 
            .FirstOrDefaultAsync();

            if (cart == null)
            {
                return new CartDto
                {
                    Items = new List<CartItemDto>()
                };
            }

            return _mapper.Map<CartDto>(cart);
        }
    }
}
