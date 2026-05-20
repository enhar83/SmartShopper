using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CommentDtos
{
    public class CommentListAdminPanelDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string CustomerFullName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;
        public byte Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsApproved { get; set; }
        public double? ToxicityScore { get; set; }
        public bool? IsToxic { get; set; }
    }
}
