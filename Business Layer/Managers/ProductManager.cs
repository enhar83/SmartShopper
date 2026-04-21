using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.ProductDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<ProductListDto>> TGetProductListAsync()
        {
            var products = await _productRepository.GetAll().ToListAsync();

            return _mapper.Map<List<ProductListDto>>(products);
        }

        public async Task TAddProductAsync(AddProductDto addProductDto)
        {
            var isExist = await _productRepository.AnyAsync(x => x.Name == addProductDto.Name);
            if (isExist)
                throw new LogicException(nameof(addProductDto.Name), "This product name is already exists.");

            var product = _mapper.Map<Product>(addProductDto);
            product.IsDeleted = false;

            await _productRepository.AddAsync(product);
            await _uow.SaveAsync();
        }
    }
}
