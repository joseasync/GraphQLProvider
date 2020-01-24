# Construindo uma API GraphQL com  .NET Core 3.0 Integrado com Azure ![](https://cdn.rawgit.com/sindresorhus/awesome/d7305f38d29fed78fa85652e3a63e154dd8e8829/media/badge.svg)

Este é um exemplo prático de como criar uma WebAPI em GraphQL em conjunto com [.NET Core 3.0](https://github.com/dotnet/aspnetcore) e hospedado pelo Azure.
De forma simples, rapida e de fácil compreensão (Melzinho na chupeta). Este projeto foi inspirado pelo [Roland Guijt](https://github.com/RolandGuijt)

*Read this in other languages: [English](README.en.md), [Svenska](README.se.md).*


## Wizard Básico
### Projeto em padrão Asp.net Core Web Application.
Para este projeto em especifico utilizei o Visual Studio 2019.\
Dentro do Wizard inicial iremos criar um novo projeto Asp.net Core Web Application.\
Posteriormente escolha o API Template (vamos aproveitar a automatização das estruturas).
![API Template](https://i.imgur.com/i36veHP.png)  
  
Fiz a estruta do projeto da forma abaixo (indico seguir a mesma para compreensão agil).
![Estrutura](https://i.imgur.com/XpP1J7n.png)



## Entidades
Para este projeto iremos utilizar o SQL Server como banco de dados com um schema básico criado no Azure.\
Caso tenha dificuldades de interação com Banco de dados no Azure, indico seguir o guia básico que fiz.\
*[Como criar um DB no Azure de forma rápida, pratica e com dados populados.](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?tabs=azure-portal)* \
(caso contrário pode optar por criar posteriormente um banco de dados local, com a mesma estrutura das entidades)


### Adicionando Entidades
Iremos utilizar duas entidades como exemplos. `Product.cs` e `ProductModel.cs` 


```csharp
[Table("Product", Schema = "SalesLT")]
public class Product
{
   public int ProductID { get; set; }
   public string Name { get; set; }
   public string ProductNumber { get; set; }
   public string Color { get; set; }
   public decimal StandardCost { get; set; }
   public decimal ListPrice { get; set; }
   public string Size { get; set; }
   public decimal? Weight { get; set; }
   public int? ProductCategoryID { get; set; }
   public int? ProductModelID { get; set; }
   public DateTime? SellStartDate { get; set; }
   public DateTime? SellEndDate { get; set; }
   public DateTime? DiscontinuedDate { get; set; }
   public string ThumbnailPhotoFileName { get; set; }
   public Guid rowguid { get; set; }
   public DateTime ModifiedDate { get; set; }
   public ProductModel ProductModel { get; set; }
}
```

```csharp
[Table("ProductModel", Schema = "SalesLT")]
public class ProductModel
{
    public int ProductModelID { get; set; }
    public string Name { get; set; }
    public string CatalogDescription { get; set; }
    public Guid rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

## Acesso a Base
Para ter acesso aos dados do nosso banco, optei por utilizar de forma simples e rápida o [EntityFramework Core.](https://github.com/dotnet/efcore)

### Criando nosso DbContext
Teremos uma classe extendendo DbContext e cujo irá expor as propriedades de DbSet que representam as nossas entidades dentro do contexto.

```csharp
public class ProviderDbContext : DbContext
{
    public ProviderDbContext(DbContextOptions<ProviderDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductModel> ProductModels { get; set; }
}
```



## Repositórios
Para definir nossas regras de inserção e busca ao banco de dados, é recomendado que cada propriedade do DbSet tenha sua classe repositório, responsável por manusear as busca.

![Repositorios](https://i.imgur.com/mVQguKV.png)

### Criando nossas classes de Repositorio
Então vamos criar no diretório *Repositories* já criado anteriormente, as nossas classes `ProductModelRepository.cs` e `ProductRepository.cs`. Dentro de nossas classes, teremos alguns métodos como por exemplo: *GetAll, GetById, AddProduct* responsáveis pelo gerenciamento do acesso ou inserção dos dados. 

[ProductModelRepository.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/Repositories/ProductModelRepository.cs)

[ProductRepository.cs](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/Repositories/ProductRepository.cs)



## GraphQL

Antes de começarmos a interagir com o GraphQL, iremos precisar adicionar alguns pacotes NuGet em nosso Projeto.\
Indico SEMPRE, utilizar as versões estáveis de cada pacote.

### [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet)  

Pacotes NuGet:

- [GraphQL](https://www.nuget.org/packages/GraphQL/2.4.0/)
- [GraphQL.Server.Transports.AspNetCore](https://www.nuget.org/packages/GraphQL.Server.Transports.AspNetCore/3.4.0)
- [GraphQL.Server.Ui.Playground](https://www.nuget.org/packages/GraphQL.Server.Ui.Playground/3.4.0)

![NuGet](https://i.imgur.com/1irWDuO.png)

### Criando nossa Estrutura GraphQL

Dentro do diretório raiz *GraphQL* iremos ter 3 classes `ProviderSchema.cs`,`ProviderQuery.cs` e `ProviderMutation.cs`.
(Gostaria de ressaltar, que ""providerCLASSE""  é o nome que eu escolhi para a compreensão do projeto, você pode escolher um nome que o ajude melhor na compreensão, ex: LojinhaSchema, LojinhaQuery :) ).
![Estrutura](https://i.imgur.com/Rq6I6hr.png)


### Schema
Iremos criar a classe `ProviderSchema.cs` extendendo a classe `Schema` provida pelo GraphQL.\
Cujo seu construtor receberá um IDependencyResolver como parâmetro (passado por injeção de dependência, veremos mais a frente).\
Todos os tipos dentro da estrutura serão resolvidos pelo IDependencyResolver. O objetivo dessa interface é fornecer flexibilidade e diminuir o acoplamento entre os componentes. Dentro da nossa classe iremos definir o tipo Mutation como `ProviderMutation` e Query como `ProviderQuery`.

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
Aqui vamos criar a classe `ProviderQuery.cs` extendendo a classe `ObjectGraphType` provida pelo GraphQL.\
Cujo seu construtor receberá nossas classes de repositório como parametro (passados por injeção de dependência dentro do escopo no setup). Dentro desta classe Query, iremos definir nossas Queries de busca e seus devidos argumentos caso necessário.\
Cada Query esta sendo representada por uma classe que extende do tipo ObjectGraphType, definindo o tipo de estrutura aguardada na requisição e retorno (vamos criar as nossas no diretório *Types* no próximo passo).

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
Também teremos nossa classe `ProviderMutation.cs` extendendo a classe `ObjectGraphType` provida pelo GraphQL.\
Cujo seu construtor receberá nossas classes de repositório como parametro como no exemplo passado. Dentro desta classe responsavel pelos Mutations, iremos definir nossas Queries de inserção e as devidas propriedades.\
Cada Mutation esta sendo representada por uma classe que extende do tipo ObjectGraphType, definindo o tipo de estrutura aguardada na requisição e como parâmetro também uma classe que extende do tipo ObjectGraphType porem para definir o tipo do objeto a ser inserido.

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
Dentro do diretório `GraphQL>Types` iremos criar nossas classes tipo "Types", responsáveis por definir os parametros de retorno e inserção requisitados nas classes anteriores (Mutations e Queries).

![Types](https://i.imgur.com/MBLR5Ej.png)




Como por exemplo as classes do tipo "Type" para *Product*.

`ProductType.cs` Responsável pelos fiels e documentação das propriedades da busca no banco.
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


`ProductInputType.cs` Responsável pelos fiels de inserção no banco.
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



## Configurações

Iremos definir agora nossa configuração de acesso ao banco de dados.

### Appsettings 

Dentro de seu `appsettings.json` configure sua ConnectionString. Indico sempre a buscar suas credênciais dentro de vossas variáveis de ambiente, pode entender como melhorar essa parte [Aqui](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows). Mas no momento de forma simples iremos adicionar diretamente o Path de nossa Conectionstring.

Exemplo:

```json
{
  "ConnectionStrings": {
    "ProviderCon": "Server=meuserverdatabase.azurewebsites.com; Database=XPTO; Persist Security Info=False;User ID=USER;Password=SENHA; MultipleActiveResultSets=true;"
  },
  "AllowedHosts": "*"
}
```

## Setup

Dentro de nosso `Setup.cs` iremos definir algumas propriedades requisitadas pelo GraphQL e Azure. Usadas tanto pelo server do GraphQL quanto do [GraphQL.Playground](https://github.com/prisma-labs/graphql-playground) utilizado para facilitar o consumo e teste de nossa API.

- [Setup](https://github.com/josecruzio/GraphQLProvider/blob/master/GraphQLProvider/Startup.cs)
  
### Método `ConfigureServices`
Em nosso Método `ConfigureServices` iremos adicionar as configurações abaixo.

Permitindo o IO assíncrono dentro de nosso ambiente no Azure, pois o default é desabilitado. 

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

Adicionando nossas classes dentro do Escopo para serem utilizadas na chamada da injeção de dependência.

```csharp
//Criando contexto por base da nossa ConnectionStringg
services.AddDbContext<ProviderDbContext>(options =>  options.UseSqlServer(Configuration["ConnectionStrings:ProviderCon"]));
services.AddScoped<ProviderSchema>();
services.AddScoped<ProductRepository>();         
services.AddScoped<ProductModelRepository>();
services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

```

Adicionando o GraphQL ao nosso serviço. 

```csharp
services.AddGraphQL(o => { o.ExposeExceptions = false; })
                    .AddGraphTypes(ServiceLifetime.Scoped)
                    .AddUserContextBuilder(httpContext => httpContext.User)
                    .AddDataLoader();
```

  
### Método `Configure`

Em nosso Método `Configure` vamos repassar à nossa IApplicationBuilder app, que utilize GraphQL e GraphQLPlayground.

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseGraphQL<ProviderSchema>();
    app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());
}
```

## Tudo Pronto :smile::relaxed:

No próximo commit, vamos conhecer a ferramenta GraphQLPlayground e como funciona o consumo de nossa GraphQL API.



## Autor
Jose Ricardo Cruz\
Software Engineer, Microsoft MCSA\
Você me encontra aqui:
 - [LinkedIn](https://www.linkedin.com/in/jrgcruz/)
 - [Twitter](https://twitter.com/josericardodev)
 - [Instagram](https://www.instagram.com/josecruz.io/)
