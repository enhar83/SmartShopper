using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProductDtos;

namespace Core_Layer.IServices
{
    public interface IProductService
    {
        Task TAddProductAsync(AddProductDto addProductDto);
    }
}
