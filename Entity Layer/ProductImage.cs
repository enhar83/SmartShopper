using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;
using Microsoft.AspNetCore.Http;

namespace Entity_Layer
{
    public class ProductImage: BaseEntity
    {
        public required string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
