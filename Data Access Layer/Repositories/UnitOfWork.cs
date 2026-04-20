using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.IRepositories;
using Data_Access_Layer.DbContext;

namespace Data_Access_Layer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public void Save() => _context.SaveChanges();

        public async Task SaveAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();

    }
}
