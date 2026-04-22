using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Core_Layer.Dtos.ProductImagesDtos
{
    public class AddProductImageDto
    {
        public Guid ProductId { get; set; }
        public required IFormFile Photo { get; set; }
        public bool IsMain { get; set; }
    }
}
