using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.IRepositories;
using Data_Access_Layer.DbContext;
using Entity_Layer;

namespace Data_Access_Layer.Repositories
{
    public class OrderItemRepository:GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
