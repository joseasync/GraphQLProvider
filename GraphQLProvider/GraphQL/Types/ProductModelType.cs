using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQLProvider.Data.Entities;
using GraphQLProvider.Repositories;

namespace GraphQLProvider.GraphQL.Types
{
    public class ProductModelType : ObjectGraphType<ProductModel>
    {
        public ProductModelType(ProductRepository productModelypeRepository, IDataLoaderContextAccessor dataLoaderContextAccessor) //, IDataLoaderContextAccessor dataLoaderContextAccessor
        {
            Field(t => t.ProductModelID);
            Field(t => t.Name).Description("Name of Model");
            Field(t => t.CatalogDescription);


            Field<ListGraphType<ProductType>>(
            "products",
            resolve: context =>
            {
                var loader = dataLoaderContextAccessor.Context.GetOrAddCollectionBatchLoader<int, Product>(
                       "GetProductsByModel", productModelypeRepository.GetProductsByModelId);
                return loader.LoadAsync(context.Source.ProductModelID);
            }
            );

        }

    }
}
