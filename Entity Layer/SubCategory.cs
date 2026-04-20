using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Entities;

namespace Entity_Layer
{
    public class SubCategory: BaseEntity
    {
        public required string Name { get; set; }
        public Guid CategoryId { get; set; }
        public required Category Category { get; set; }
    }
}
