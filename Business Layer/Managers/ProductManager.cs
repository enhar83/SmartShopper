using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core_Layer.Dtos.ProductDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class ProductManager : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductManager(
            IProductRepository productRepository,
            IFavoriteRepository favoriteRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _favoriteRepository = favoriteRepository;
            _uow = uow;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ProductListDto>> TGetProductListForIndex()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var products = await _productRepository.GetAll()
                .Include(p => p.SubCategory)
                    .ThenInclude(c => c!.Category)
                .Include(p => p.ProductImages)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            HashSet<Guid> userFavoriteIds = new HashSet<Guid>();
            if (currentUserId.HasValue)
            {
                var favs = await _favoriteRepository
                    .Where(x => x.AppUserId == currentUserId.Value)
                    .Select(x => x.ProductId)
                    .ToListAsync();
                userFavoriteIds = new HashSet<Guid>(favs);
            }

            var mappedList = _mapper.Map<List<ProductListDto>>(products);

            for (int i = 0; i < products.Count; i++)
            {
                var original = products[i];
                var target = mappedList[i];

                if (original.Stock <= 0)
                {
                    target.StockStatus = "Out of Stock";
                    target.StockStatusClass = "bg-danger";
                }
                else if (original.Stock <= 50)
                {
                    target.StockStatus = "Limited";
                    target.StockStatusClass = "bg-warning text-dark";
                }
                else
                {
                    target.StockStatus = "In Stock";
                    target.StockStatusClass = "bg-success";
                }

                target.IsFavorite = userFavoriteIds.Contains(original.Id);
            }

            return mappedList;
        }

        public async Task<List<ProductListDtoAdminPanel>> TGetProductListAsync()
        {
            return await _productRepository.GetAll()
                .ProjectTo<ProductListDtoAdminPanel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task TAddProductAsync(AddProductDto addProductDto)
        {
            var isExist = await _productRepository.AnyAsync(x => x.Name == addProductDto.Name);
            if (isExist)
                throw new LogicException(nameof(addProductDto.Name), "This product name already exists.");

            var product = _mapper.Map<Product>(addProductDto);
            product.CreatedDate = DateTime.Now;
            product.IsDeleted = false;

            await _productRepository.AddAsync(product);
            await _uow.SaveAsync();
        }

        public async Task TUpdateProductAsync(UpdateProductDto updateProductDto)
        {
            var productToUpdate = await _productRepository.GetByIdAsync(updateProductDto.Id);
            if (productToUpdate == null)
                throw new LogicException(nameof(updateProductDto.Id), "The product not found.");

            var anyExists = await _productRepository.AnyAsync(p => p.Id != updateProductDto.Id && p.Name == updateProductDto.Name);
            if (anyExists)
                throw new LogicException(nameof(updateProductDto.Name), "This product name already exists.");

            _mapper.Map(updateProductDto, productToUpdate);
            productToUpdate.UpdatedDate = DateTime.Now;

            _productRepository.Update(productToUpdate);
            await _uow.SaveAsync();
        }

        public async Task<ProductListDtoAdminPanel> TGetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new LogicException(nameof(ProductListDtoAdminPanel.Id), "The product not found");

            return _mapper.Map<ProductListDtoAdminPanel>(product);
        }

        public async Task<ProductDetailDto> TGetProductDetailsAsync(Guid id)
        {
            var product = await _productRepository.Where(x => x.Id == id)
                .Include(x => x.ProductImages)
                .Include(x => x.SubCategory)
                    .ThenInclude(s => s!.Category)
                .FirstOrDefaultAsync();

            if (product == null)
                throw new LogicException("Id", "The product not found.");

            var dto = _mapper.Map<ProductDetailDto>(product);

            if (product.Stock <= 0) { dto.StockStatus = "Out of Stock"; dto.StockStatusClass = "text-danger"; }
            else if (product.Stock <= 50) { dto.StockStatus = "Limited Stock!"; dto.StockStatusClass = "text-warning"; }
            else { dto.StockStatus = "In Stock"; dto.StockStatusClass = "text-success"; }

            return dto;
        }

        public async Task<List<SimilarProductsForProductDetailDto>> TGetSimilarProductsForProductDetailAsync(Guid subCategoryId)
        {
            var products = await _productRepository.Where(x => x.SubCategoryId == subCategoryId && !x.IsDeleted)
                .Include(x => x.SubCategory)
                    .ThenInclude(s => s!.Category)
                .Include(x => x.ProductImages)
                .Take(10)
                .ToListAsync();

            var mappedList = _mapper.Map<List<SimilarProductsForProductDetailDto>>(products);
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                var d = mappedList[i];

                if (p.Stock <= 0) { d.StockStatus = "Out of Stock"; d.StockStatusClass = "bg-danger"; }
                else if (p.Stock <= 50) { d.StockStatus = "Limited"; d.StockStatusClass = "bg-warning text-dark"; }
                else { d.StockStatus = "In Stock"; d.StockStatusClass = "bg-success"; }
            }

            return mappedList;
        }
    }
}