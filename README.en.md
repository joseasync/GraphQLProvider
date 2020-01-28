# Building a GraphQL API with .NET Core 3.0 Integrated with Azure. ![](https://cdn.rawgit.com/sindresorhus/awesome/d7305f38d29fed78fa85652e3a63e154dd8e8829/media/badge.svg)

This is a practical example of how to create a WebAPI in GraphQL in conjunction with [.NET Core 3.0](https://github.com/dotnet/aspnetcore) and hosted by Azure. On a simple ands traight way. This project was inspired by [Roland Guijt](https://github.com/RolandGuijt)

* Read this in other languages: [Portuguese](README.md), [Svenska](README.se.md). *


## Basic Wizard
### Project in standard Asp.net Core Web Application.

For this specific project I used Visual Studio 2019.\
Chose the option "Create New Project", on the initial Wizard we will create a new Asp.net Core Web Application project. \
Later choose the API Template (we will take advantage of the automation to create the structures).
![API Template](https://i.imgur.com/i36veHP.png)

I structured the project with specifics paths as you can see below (I recommend following it to help your comprehension).
![Structure](https://i.imgur.com/XpP1J7n.png)
 

## Entities
For this project we will use SQL Server as a database with a basic schema created in Azure. \
If you have difficulties interacting with Database in Azure, I recommend following the basic guide I did.\
*[How to create a DB in Azure quickly, conveniently and with populated data](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?tabs=azure-portal)*\
(If you prefer, you can choose to create a local database later, with the same structure as the entities)


### Adding Entities
We will use two entities as examples. `Product.cs` and `ProductModel.cs`

```csharp
[Table ("Product", Schema = "SalesLT")]
public class Product
{
   public int ProductID {get; set; }
   public string Name {get; set; }
   public string ProductNumber {get; set; }
   public string Color {get; set; }
   public decimal StandardCost {get; set; }
   public decimal ListPrice {get; set; }
   public string Size {get; set; }
   public decimal? Weight {get; set; }
   public int? ProductCategoryID {get; set; }
   public int? ProductModelID {get; set; }
   public DateTime? SellStartDate {get; set; }
   public DateTime? SellEndDate {get; set; }
   public DateTime? DiscontinuedDate {get; set; }
   public string ThumbnailPhotoFileName {get; set; }
   public Guid rowguid {get; set; }
   public DateTime ModifiedDate {get; set; }
   public ProductModel ProductModel {get; set; }
}
```

``` csharp
[Table ("ProductModel", Schema = "SalesLT")]
public class ProductModel
{
    public int ProductModelID {get; set; }
    public string Name {get; set; }
    public string CatalogDescription {get; set; }
    public Guid rowguid {get; set; }
    public DateTime ModifiedDate {get; set; }
}
```

## Access to Database
To access our database, I opted to use the [EntityFramework Core.](https://github.com/dotnet/efcore), an easy way to understand the structure.
    
### Creating our DbContext
We will have a class extending DbContext whose will expose the properties of DbSet that represent our entities in the context.

```csharp
public class ProviderDbContext: DbContext
{
    public ProviderDbContext (DbContextOptions <ProviderDbContext> options): base (options) {}
    public DbSet <Product> Products {get; set; }
    public DbSet <ProductModel> ProductModels {get; set; }
}
```


## Repositories
To define our rules for inserting and searching on the database, is recommended for each DbSet property, one repository class, responsible for handling the queries.

![Repositories](https://i.imgur.com/mVQguKV.png)

### Creating our Repository classes
So let's create in the *Repositories* directory already created, our `ProductModelRepository.cs` and `ProductRepository.cs` classes. Within our classes, we will have some methods such as: *GetAll, GetById, AddProduct* responsible for managing the access or insert the data.

[ProductModelRepository.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/Repositories/ProductModelRepository.cs)

[ProductRepository.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/Repositories/ProductRepository.cs)


## GraphQL

Before we start to interact with GraphQL, we will need to add some 1NuGet packages to our Project.\
I recomend to use the stable versions of each package.

### [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet)

NuGet packages:

- [GraphQL](https://www.nuget.org/packages/GraphQL/2.4.0/)
- [GraphQL.Server.Transports.AspNetCore](https://www.nuget.org/packages/GraphQL.Server.Transports.AspNetCore/3.4.0)
- [GraphQL.Server.Ui.Playground](https://www.nuget.org/packages/GraphQL.Server.Ui.Playground/3.4.0)

![NuGet](https://i.imgur.com/1irWDuO.png)

### Creating our GraphQL Structure

Within the *GraphQL* root directory we will have 3 classes `ProviderSchema.cs`, `ProviderQuery.cs` and `ProviderMutation.cs`.
(I would like to point out, that ""providerCLASSE"" is the name that I chose to help on the comprehension of the project, you can choose a name that helps you to improve the comprehension, eg MyStoreSchema, MyStoreQuery :relaxed:).

![Structure](https://i.imgur.com/Rq6I6hr.png)

### Schema
We will create the `ProviderSchema.cs` class whose extends the `Schema` class provided by GraphQL.\
The constructor will receive an IDependencyResolver as a parameter (passed through dependency injection, we'll see later).\
All types within the structure will be resolved by IDependencyResolver. The purpose of this interface is to provide flexibility and decrease coupling between components. In our class we will define the type Mutation as `ProviderMutation` and Query as `ProviderQuery`.


```csharp
public class ProviderSchema : Schema
{
    public ProviderSchema(IDependencyResolver resolver) : base(resolver)
    {
        Query = resolver.Resolve<ProviderQuery>();
        Mutation = resolver.Resolve<ProviderMutation>();
    }
}
```
### Query
Here we will create the `ProviderQuery.cs` class extending the `ObjectGraphType` class provided by GraphQL.\
The constructor will receive our repository classes as a parameter (passed through dependency injection within the scope of the setup). In our Query class, we will define our search queries and their arguments if necessary.\
Each Query is being represented by a class that extends the ObjectGraphType type, defining the type of structure expected in the request and return (we will create ours definitions in the *Types* directory in the next step).


```csharp
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
            return _productModelId == null ? productModelRepository.GetAll() :productModelRepository.GetById(_productModelId.Value);
        });

    }
}
```

### Mutation
We will also have our `ProviderMutation.cs` class extending the `ObjectGraphType` class provided by GraphQL.\
The constructor will receive our repository classes as a parameter such the previous example. This class is responsible for Mutations, whose define our Insertion Queries and the appropriate properties.\
Each Mutation is being represented by a class that extends the ObjectGraphType type, defining the type of structure expected in the request and as a parameter also one class that extends the ObjectGraphType type, to define the type of the object to be inserted.


```csharp
public class ProviderMutation : ObjectGraphType
{
    public ProviderMutation(ProductModelRepository productModelRepository, ProductRepository productRepository)
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

        FieldAsync<ProductType>(
           "createProduct",
           arguments: new QueryArguments(
               new QueryArgument<NonNullGraphType<ProductInputType>> { Name = "product" }),
               resolve: async context =>
               {
                   var product = context.GetArgument<Product>("product");
                   return await context.TryAsyncResolve(
                       async c => await productRepository.AddProduct(product));
               }
           );
    }
}
```

### Types
Within the `GraphQL>Types` directory we will create our "Types" classes, responsible for defining the return and insertion parameters required in the previous classes (Mutations and Queries).

![Types](https://i.imgur.com/MBLR5Ej.png)

For example, classes of type "Type" for *Product*.

`ProductType.cs` Responsible for the fields and documentation of the search properties.

```csharp
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
```


`ProductInputType.cs` Responsible for the fields and documentation of the insert.

```csharp
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
```

- [ProductInputType.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/GraphQL/Types/ProductInputType.cs)
- [ProductModelInputType.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/GraphQL/Types/ProductModelInputType.cs)
- [ProductModelType.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/GraphQL/Types/ProductModelType.cs)
- [ProductType.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/GraphQL/Types/ProductType.cs)


## Settings

We will now define our database access configuration.

### Appsettings

Open your `appsettings.json` and configure the ConnectionString. I always recommend store your credentials within your environment variables, you can understand how to improve this part [Here](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows). But for the moment in a simple way, we will directly add the Path of our Conectionstring.

Example:

```json
{
  "ConnectionStrings": {
    "ProviderCon": "Server=meuserverdatabase.azurewebsites.com; Database=XPTO; Persist Security Info=False;User ID=USER;Password=SENHA; MultipleActiveResultSets=true;"
  },
  "AllowedHosts": "*"
}
```

## Setup

On `Setup.cs` we will define some properties requested by GraphQL and Azure. Used by both the GraphQL server and the [GraphQL.Playground](https://github.com/prisma-labs/graphql-playground) used to facilitate the consumption and testing of our API.


- [Setup](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/Startup.cs)

### `ConfigureServices` method
In our `ConfigureServices` Method we will add the settings below.

Allowing asynchronous IO within our Azure environment, as the default is disabled.

```csharp
//Usando Kestrel
services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

//Usando IIS
services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
```

Adding our classes on the Scope to be used in the dependency injection.

```csharp
//Criando contexto por base da nossa ConnectionStringg
services.AddDbContext<ProviderDbContext>(options =>  options.UseSqlServer(Configuration["ConnectionStrings:ProviderCon"]));
services.AddScoped<ProviderSchema>();
services.AddScoped<ProductRepository>();         
services.AddScoped<ProductModelRepository>();
services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

```

Adding GraphQL to our service.


```csharp
services.AddGraphQL(o => { o.ExposeExceptions = false; })
                    .AddGraphTypes(ServiceLifetime.Scoped)
                    .AddUserContextBuilder(httpContext => httpContext.User)
                    .AddDataLoader();
```

### `Configure` method

In our `Configure` Method we will set on our IApplicationBuilder app, to uses GraphQL and GraphQLPlayground.

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseGraphQL<ProviderSchema>();
    app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());
}
```

## Wa are ready! :smile: :relaxed:

Starting our application, we can check our GraphQL.Playground page at https://{yourlocalhost}/ui/playground, working and consuming our endpoint, as we can see below.

![Playground](https://i.imgur.com/22rrNkD.png)


### Search

We can now search for the data, requesting only the desired fields, and based on our filter, by the product id.
If you want to bring all the products just remove *(id: 706)* used as a filter. Remove and add the desired fields to improve your  comprehension.


```javascript
{
  products(id:706){
   				productID,
          name,
          productNumber,
          color,
          standardCost,
          listPrice,
          size,
          weight,
          productCategoryID,
          productModelID,
          sellStartDate,
          sellEndDate,
          discontinuedDate,
          thumbnailPhotoFileName,
          modifiedDate
  }
}
```


Remembering that we can work with sub-entities, as in the example below, looking for productmodels with products that have their respective model.


```javascript
{
  productModels{
    productModelID,
   		name,
    	products{
        productID,
          name,
          productNumber,
      }
  }
}
```


### Insert

In the same way, we can insert new data through our multations in our DB.
Set our data structure in the query tab and the respective values in the *Query Variables* tab.

![insert](https://i.imgur.com/RHrMJSw.png)


### Conclusions

I hope you enjoyed this tutorial to Build a GraphQL with .NET Core in a simple and direct way, any questions you can find me on the links below.

## Author
Jose Ricardo Cruz\
Software Engineer, Microsoft MCSA\
You find me here:
 - [LinkedIn](https://www.linkedin.com/in/jrgcruz/)
 - [Twitter](https://twitter.com/josericardodev)
 - [Instagram](https://www.instagram.com/josecruz.io/)

