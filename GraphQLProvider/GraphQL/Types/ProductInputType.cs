using GraphQL.Types;

namespace GraphQLProvider.GraphQL.Types
{
    
    public class ProductInputType : InputObjectGraphType
    {
        public ProductInputType()
        {
            Name = "inputproduct";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("productNumber");
            Field<StringGraphType>("color");
            Field<StringGraphType>("catalogDescription");
            Field<NonNullGraphType<DecimalGraphType>>("standardCost");
            Field<NonNullGraphType<DecimalGraphType>>("listPrice");
            Field<NonNullGraphType<StringGraphType>>("size");
            Field<DecimalGraphType>("weight");
            Field<IntGraphType>("productCategoryID");
            Field<IntGraphType>("productModelID");
            Field<DateGraphType>("sellStartDate");
            Field<DateGraphType>("sellEndDate");
            Field<DateGraphType>("discontinuedDate");
            Field<StringGraphType>("thumbnailPhotoFileName");
        }
    }
}
