using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.CommentToxicityModels
{
    public class ToxicityModelInput
    {
        [LoadColumn(0)]
        [ColumnName("Text")]
        public string Text { get; set; } = null!;
    }
}
