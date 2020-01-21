using GraphQLProvider.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace GraphQLProvider.Data.Context
{
    public class ProviderDbContext : DbContext
    {
        public ProviderDbContext(DbContextOptions<ProviderDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductModel> ProductModels { get; set; }
    }
}
