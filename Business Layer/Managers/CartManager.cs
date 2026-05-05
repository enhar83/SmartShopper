using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CartDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class CartManager : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CartManager(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository, IUnitOfWork uow, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task AddToCartAsync(Guid userId, CreateCartItemDto createCartItemDto)
        {
            var product = await _productRepository.GetByIdAsync(createCartItemDto.ProductId);
            if (product == null || product.Stock < createCartItemDto.Quantity)
                throw new LogicException("Quantity","Product out of stock or does not exist.");

            var cart = await _cartRepository.GetAsync(x => x.AppUserId == userId); 
        
            if (cart == null)
            {
                cart = new Cart { AppUserId = userId }; 
                await _cartRepository.AddAsync(cart);
            }

            var existingItem = await _cartItemRepository.GetAsync(x =>
                x.CartId == cart.Id && x.ProductId == createCartItemDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += createCartItemDto.Quantity; 
                _cartItemRepository.Update(existingItem);
            }
            else
            {
                var newCartItem = _mapper.Map<CartItem>(createCartItemDto); 
                newCartItem.CartId = cart.Id; 
                await _cartItemRepository.AddAsync(newCartItem);
            }

            await _uow.SaveAsync();
        }

        public async Task<CartDto> TGetUserCartAsync(Guid userId)
        {
            var query = _cartRepository.Where(x => x.AppUserId == userId);

        var cart = await query
            .Include(c => c.CartItems)           
                .ThenInclude(ci => ci.Product)     
                    .ThenInclude(p => p!.ProductImages) 
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
