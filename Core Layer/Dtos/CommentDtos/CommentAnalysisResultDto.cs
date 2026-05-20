using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CommentDtos
{
    public class CommentAnalysisResultDto
    {
        public double ToxicityScore { get; set; }
        public bool IsToxic { get; set; }
        public double SentimentScore { get; set; }
    }
}
