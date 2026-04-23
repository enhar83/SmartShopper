using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProductImagesDtos;

namespace Core_Layer.IServices
{
    public interface IProductImageService
    {
        Task TAddProductImageAsync(AddProductImageDto addProductImageDto);
        Task<List<ProductImageListDto>> TGetProductImagesByProductIdAsync(Guid productId);
        Task TDeleteProductImageAsync(Guid id);
    }
}
