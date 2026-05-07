using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Data_Access_Layer.DbContext;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Data_Access_Layer.Seeds
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;

        public DataSeeder(AppDbContext context)
        {
            _context = context;
        }
    }
}