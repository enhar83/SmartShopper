using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class ProductImage: BaseEntity
    {
        public required string ImageUrl { get; set; }
        public Guid ProductId { get; set; }
        public required Product Product { get; set; }
    }
}
