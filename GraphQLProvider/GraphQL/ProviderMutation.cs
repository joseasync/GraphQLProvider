using GraphQL.Types;
using GraphQLProvider.Data.Entities;
using GraphQLProvider.GraphQL.Types;
using GraphQLProvider.Repositories;

namespace GraphQLProvider.GraphQL
{
    public class ProviderMutation : ObjectGraphType
    {
        public ProviderMutation(ProductModelRepository productModelRepository)
        {
            FieldAsync<ProductModelType>(
                "createModel",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ProductModelInputType>> { Name = "model" }),
                    resolve: async context =>
                    {
                        var model = context.GetArgument<ProductModel>("model");
                        return await context.TryAsyncResolve(
                            async c => await productModelRepository.AddProductModel(model));
                    }
                );
        }
    }
}
