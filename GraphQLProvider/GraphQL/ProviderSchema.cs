using GraphQL;
using GraphQL.Types;

namespace GraphQLProvider.GraphQL
{
    public class ProviderSchema : Schema
    {
        public ProviderSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<ProviderQuery>();
            Mutation = resolver.Resolve<ProviderMutation>();
        }
    }
}
 