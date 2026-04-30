using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.ProductDtos;

namespace Core_Layer.IServices
{
    public interface IProductService
    {
        Task<ProductListDtoAdminPanel> TGetByIdAsync(Guid id);
        Task TAddProductAsync(AddProductDto addProductDto);
        Task<List<ProductListDtoAdminPanel>> TGetProductListAsync();
        Task TUpdateProductAsync(UpdateProductDto updateProductDto);
        Task<List<ProductListDto>> TGetProductListForIndex();
    }
}
