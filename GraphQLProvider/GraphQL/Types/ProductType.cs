using GraphQL.Types;
using GraphQLProvider.Data.Entities;

namespace GraphQLProvider.GraphQL.Types
{
    public class ProductType : ObjectGraphType<Product>
    {
        public ProductType()
        {
            Field(t => t.ProductID);
            Field(t => t.Name).Description("The name of the product");
            Field(t => t.ProductNumber);
            Field(t => t.Color).Description("Color"); ;
            Field(t => t.StandardCost);
            Field(t => t.ListPrice);
            Field(t => t.Size);
            Field(t => t.Weight, nullable: true);
            Field(t => t.ProductCategoryID, nullable: true);
            Field(t => t.ProductModelID, nullable: true);
            Field(t => t.SellStartDate, nullable: true);
            Field(t => t.SellEndDate, nullable: true);
            Field(t => t.DiscontinuedDate, nullable: true);
            Field(t => t.ThumbnailPhotoFileName);
            Field(t => t.ModifiedDate);
            
        }

    }
}
