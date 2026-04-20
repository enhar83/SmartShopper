using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.IRepositories
{
    public interface IUnitOfWork: IDisposable
    {
        Task SaveAsync();
        void Save();
    }
}
