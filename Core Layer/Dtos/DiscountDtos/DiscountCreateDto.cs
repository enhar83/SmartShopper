using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entity_Layer.Discount;

namespace Core_Layer.Dtos.DiscountDtos
{
    public class DiscountCreateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
