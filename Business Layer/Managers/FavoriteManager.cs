using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.FavoriteDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class FavoriteManager : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public FavoriteManager(IFavoriteRepository favoriteRepository, IProductRepository productRepository, IUnitOfWork uow, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<List<WishlistDto>> TGetWishListAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new LogicException("Auth", "Please sign in.");

            var currentUserId = Guid.Parse(userIdClaim);

            var favorites = await _favoriteRepository.Where(x => x.AppUserId == currentUserId)
                .Include(x => x.Product)
                    .ThenInclude(p => p.SubCategory)
                        .ThenInclude(s => s!.Category)
                .Include(x => x.Product.ProductImages)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var mappedList = _mapper.Map<List<WishlistDto>>(favorites);

            for (int i = 0; i < favorites.Count; i++)
            {
                var originalProduct = favorites[i].Product;
                var targetDto = mappedList[i];

                if (originalProduct.Stock <= 0)
                {
                    targetDto.StockStatus = "Stokta Yok";
                    targetDto.StockStatusClass = "bg-danger";
                }
                else if (originalProduct.Stock <= 50)
                {
                    targetDto.StockStatus = "Kritik Stok";
                    targetDto.StockStatusClass = "bg-warning text-dark";
                }
                else
                {
                    targetDto.StockStatus = "Stokta Var";
                    targetDto.StockStatusClass = "bg-success";
                }
            }

            return mappedList;
        }

        public async Task<bool> TToggleFavoriteAsync(Guid productId)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new LogicException("Auth", "User session not found.");

            var currentUserId = Guid.Parse(userIdClaim);

            var productExists = await _productRepository.AnyAsync(x => x.Id == productId);
            if (!productExists)
                throw new LogicException("NotFound", "Product not found.");

            var existingFavorite = await _favoriteRepository.GetAsync(x =>
                x.AppUserId == currentUserId && x.ProductId == productId);

            if (existingFavorite != null)
            {
                _favoriteRepository.Remove(existingFavorite);
                await _uow.SaveAsync();
                return false; 
            }
            else
            {
                var newFavorite = new Favorite
                {
                    Id = Guid.NewGuid(),
                    AppUserId = currentUserId,
                    ProductId = productId,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _favoriteRepository.AddAsync(newFavorite);
                await _uow.SaveAsync();
                return true; 
            }
        }
    }
}
