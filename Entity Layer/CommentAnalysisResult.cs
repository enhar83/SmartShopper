using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class CommentAnalysisResult:BaseEntity
    {
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;
        public double ToxicityScore { get; set; }
        public bool IsToxic { get; set; }
        public double SentimentScore { get; set; }
    }
}
