using GraphQLProvider.Data.Context;
using GraphQLProvider.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace GraphQLProvider.Repositories
{
    public class ProductModelRepository
    {
        private readonly ProviderDbContext _dbContext;

        public ProductModelRepository(ProviderDbContext dbContext) => _dbContext = dbContext;

        public async Task<List<ProductModel>> GetAll() => await _dbContext.ProductModels.ToListAsync();

        public async Task<List<ProductModel>> GetById(int id) => await _dbContext.ProductModels.Where(p => p.ProductModelID == id).ToListAsync();

        public async Task<ProductModel> AddProductModel(ProductModel model)
        {
            model.rowguid = Guid.NewGuid();
            model.ModifiedDate = DateTime.Now;
            _dbContext.ProductModels.Add(model);
            await _dbContext.SaveChangesAsync();
            return model;
        }
    }
}