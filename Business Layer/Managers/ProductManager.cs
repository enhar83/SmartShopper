using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.ProductDtos;
using Core_Layer.Dtos.RoleDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Business_Layer.Managers
{
    public class ProductManager : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProductManager(IProductRepository productRepository, IUnitOfWork uow, IMapper mapper)
        {
            _productRepository = productRepository;
            _uow = uow;
            _mapper = mapper;
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
                throw new LogicException(nameof(addProductDto.Name), "This product name is already exists.");

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

            var anyExists = await _productRepository.AnyAsync(p=>p.Id != updateProductDto.Id && p.Name == updateProductDto.Name);
            if (anyExists)
                throw new LogicException(nameof(updateProductDto.Name), "This product name is already exists.");

            _mapper.Map(updateProductDto,productToUpdate);
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

        public async Task<List<ProductListDto>> TGetProductListForIndex()
        {
            var products = await _productRepository.GetAll()
                .Include(p => p.SubCategory)
                .ThenInclude(c=>c!.Category)
                .Include(p => p.ProductImages) 
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var mappedList = _mapper.Map<List<ProductListDto>>(products);

            for (int i = 0; i < products.Count; i++)
            {
                var originalProduct = products[i];
                var targetDto = mappedList[i];

                if (originalProduct.Stock <= 0)
                {
                    targetDto.StockStatus = "Out of Stock";
                    targetDto.StockStatusClass = "bg-danger"; 
                }
                else if (originalProduct.Stock <= 50)
                {
                    targetDto.StockStatus = "Limited";
                    targetDto.StockStatusClass = "bg-warning text-dark"; 
                }
                else
                {
                    targetDto.StockStatus = "In Stock";
                    targetDto.StockStatusClass = "bg-success";
                }
            }

            return mappedList;
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

            if (product.Stock <= 0)
            {
                dto.StockStatus = "Out of Stock";
                dto.StockStatusClass = "text-danger";
            }
            else if (product.Stock <= 50)
            {
                dto.StockStatus = "Limited Stock! (Hurry up, almost gone)";
                dto.StockStatusClass = "text-warning";
            }
            else
            {
                dto.StockStatus = "In Stock";
                dto.StockStatusClass = "text-success";
            }

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
                var product = products[i];
                var dto = mappedList[i];

                if (product.Stock <= 0)
                {
                    dto.StockStatus = "Out of Stock";
                    dto.StockStatusClass = "bg-danger";
                }
                else if (product.Stock <= 50)
                {
                    dto.StockStatus = "Limited";
                    dto.StockStatusClass = "bg-warning text-dark";
                }
                else
                {
                    dto.StockStatus = "In Stock";
                    dto.StockStatusClass = "bg-success";
                }
            }

            return mappedList;
        }
    }
}
