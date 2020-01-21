using GraphQL.Types;

namespace GraphQLProvider.GraphQL.Types
{
    public class ProductModelInputType : InputObjectGraphType
    {
        public ProductModelInputType()
        {
            Name = "inputproductmodel";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<StringGraphType>("catalogDescription");
        }
    }
}
