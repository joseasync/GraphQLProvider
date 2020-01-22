using GraphQLProvider.Data.Context;
using GraphQLProvider.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace GraphQLProvider.Repositories
{
    public class ProductRepository
    {
        private readonly ProviderDbContext _dbContext;

        public ProductRepository(ProviderDbContext dbContext) => _dbContext = dbContext;

        public async Task<List<Product>> GetAll() => await _dbContext.Products.ToListAsync();

        public async Task<List<Product>> GetById(int id) => await _dbContext.Products.Where(p => p.ProductID == id).ToListAsync();

        public async Task<ILookup<int, Product>> GetProductsByModelId(IEnumerable<int> productModelIds)
        {
            var products = await _dbContext.Products.Where(
               p => productModelIds.Contains(p.ProductModelID.Value)).ToListAsync();

            return products.ToLookup(p => p.ProductModelID.Value);
        }
        public async Task<Product> AddProduct(Product product)
        {
            product.rowguid = Guid.NewGuid();
            product.ModifiedDate = DateTime.Now;
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }
    }
}
