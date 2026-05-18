using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CommentDtos
{
    public class CreateCommentDto
    {
        public Guid ProductId { get; set; }
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;   
        public byte Rating { get; set; }
    }
}
