using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;


namespace Entity_Layer
{
    public class CartItem:BaseEntity
    {
        public Guid CartId {  get; set; }
        public required Cart Cart { get; set; }

        public Guid ProductId { get; set; }
        public required Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
