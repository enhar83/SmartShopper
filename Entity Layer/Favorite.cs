using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class Favorite : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public required AppUser AppUser { get; set; }
        public Guid ProductId { get; set; }
        public required Product Product { get; set; }
    }
}
