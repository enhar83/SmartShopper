using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using Microsoft.AspNetCore.Http;

namespace Business_Layer.Managers
{
    public class FavoriteManager : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FavoriteManager(IFavoriteRepository favoriteRepository, IProductRepository productRepository, IUnitOfWork uow, IHttpContextAccessor httpContextAccessor)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> TTogleFavoriteAsync(Guid productId)
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
