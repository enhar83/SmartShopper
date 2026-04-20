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
    public class SubCategoryRepository:GenericRepository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
