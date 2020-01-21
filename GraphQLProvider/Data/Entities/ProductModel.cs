using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQLProvider.Data.Entities
{
    [Table("ProductModel", Schema = "SalesLT")]
    public class ProductModel
    {
        public int ProductModelID { get; set; }
        public string Name { get; set; }
        public string CatalogDescription { get; set; }
        public Guid rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
