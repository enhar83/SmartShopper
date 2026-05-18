using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class Comment:BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public byte Rating { get; set; }
        public bool IsApproved { get; set; } = false;
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
