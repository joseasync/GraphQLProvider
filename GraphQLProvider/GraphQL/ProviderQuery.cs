using GraphQL.Types;
using GraphQLProvider.GraphQL.Types;
using GraphQLProvider.Repositories;

namespace GraphQLProvider.GraphQL
{
    public class ProviderQuery : ObjectGraphType
    {
        public ProviderQuery(ProductRepository productRepository, ProductModelRepository productModelRepository)
        {
            Field<ListGraphType<ProductType>>(
           "products",
            arguments: new QueryArguments(new QueryArgument<IdGraphType> { Name = "id" }),
           resolve: context =>
           {
                var _productId = context.GetArgument<int?>("id");
                return _productId == null ? productRepository.GetAll() : productRepository.GetById(_productId.Value);

           });

            Field<ListGraphType<ProductModelType>>(
           "productModels",
            arguments: new QueryArguments(new QueryArgument<IdGraphType> { Name = "id" }),
            resolve: context =>
            {
                var _productModelId = context.GetArgument<int?>("id");
                return _productModelId == null ? productModelRepository.GetAll() : productModelRepository.GetById(_productModelId.Value);
            });

        }

    }
}
