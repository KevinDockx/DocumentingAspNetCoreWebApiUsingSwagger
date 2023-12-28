using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Library.API;
using Library.API.Authentication;
using Library.API.Contexts;
using Library.API.OperationFilters;
using Library.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Reflection;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(configure =>
{
    configure.ReturnHttpNotAcceptable = true;

    configure.Filters.Add(
       new ProducesResponseTypeAttribute(
           StatusCodes.Status400BadRequest));
    configure.Filters.Add(
        new ProducesResponseTypeAttribute(
            StatusCodes.Status406NotAcceptable));
    configure.Filters.Add(
        new ProducesResponseTypeAttribute(
            StatusCodes.Status500InternalServerError));
    configure.Filters.Add(
        new ProducesResponseTypeAttribute(
            StatusCodes.Status401Unauthorized));

}).AddNewtonsoftJson(setupAction =>
{
    setupAction.SerializerSettings.ContractResolver =
       new CamelCasePropertyNamesContractResolver();
}).AddXmlDataContractSerializerFormatters(); 

// configure the NewtonsoftJsonOutputFormatter
builder.Services.Configure<MvcOptions>(configureOptions => 
{
    var jsonOutputFormatter = configureOptions.OutputFormatters
        .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

    // remove text/json as it isn't the approved media type
    // for working with JSON at API level
    jsonOutputFormatter?.SupportedMediaTypes.Remove("text/json");

    configureOptions.Filters.Add(new AuthorizeFilter());
});

// configure basic authentication 
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, 
    BasicAuthenticationHandler>("Basic", null);

builder.Services.AddDbContext<LibraryContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:LibraryDBConnectionString"]));

builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
}).AddMvc()
.AddApiExplorer(setupAction =>
{
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.SubstituteApiVersionInUrl = true;
});

var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
  .GetRequiredService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(setupAction =>
    {
        //setupAction.SwaggerDoc("LibraryOpenAPISpecificationAuthors", new()
        //{
        //    Title = "Library API (Authors)",
        //    Version = "1",
        //    Description = "Through this API you can access authors",
        //    Contact = new()
        //    {
        //        Email = "kevin.dockx@gmail.com",
        //        Name = "Kevin Dockx",
        //        Url = new Uri("https://www.twitter.com/KevinDockx")
        //    },
        //    License = new()
        //    {
        //        Name = "MIT License",
        //        Url = new Uri("https://opensource.org/licenses/MIT")
        //    }
        //});

        //setupAction.SwaggerDoc("LibraryOpenAPISpecificationBooks", new()
        //{
        //    Title = "Library API (Books)",
        //    Version = "1",
        //    Description = "Through this API you can access books",
        //    Contact = new()
        //    {
        //        Email = "kevin.dockx@gmail.com",
        //        Name = "Kevin Dockx",
        //        Url = new Uri("https://www.twitter.com/KevinDockx")
        //    },
        //    License = new()
        //    {
        //        Name = "MIT License",
        //        Url = new Uri("https://opensource.org/licenses/MIT")
        //    }
        //});

        foreach (var description in
            apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            setupAction.SwaggerDoc(
                $"{description.GroupName}",
                new()
                {
                    Title = "Library API",
                    Version = description.ApiVersion.ToString(),
                    Description = "Through this API you can access authors and their books.",
                    Contact = new()
                    {
                        Email = "kevin.dockx@gmail.com",
                        Name = "Kevin Dockx",
                        Url = new Uri("https://www.twitter.com/KevinDockx")
                    },
                    License = new()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
        }

        setupAction.AddSecurityDefinition("basicAuth", 
            new ()
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Description = "Input your username and password to access this API"
            });

        setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basicAuth" }
                        }, new List<string>() }
                });


        //setupAction.ResolveConflictingActions(apiDescriptions =>
        //{
        //    return apiDescriptions.First();
        //});

        setupAction.OperationFilter<GetBookOperationFilter>();
        setupAction.OperationFilter<CreateBookOperationFilter>();

        var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlCommentsFullPath = Path.Combine(
            AppContext.BaseDirectory, xmlCommentsFile);

        setupAction.IncludeXmlComments(xmlCommentsFullPath);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();

app.UseSwaggerUI(setupAction =>
{
    //setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationAuthors/swagger.json", 
    //        "Library API (Authors)");
    //setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationBooks/swagger.json", 
    //        "Library API (Books)");
    var descriptions = app.DescribeApiVersions();
    foreach (var description in descriptions)
    {
        setupAction.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
    setupAction.RoutePrefix = string.Empty;
    setupAction.DefaultModelExpandDepth(2);
    setupAction.DefaultModelRendering(
        Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
    setupAction.DocExpansion(
        Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    setupAction.EnableDeepLinking();
    setupAction.DisplayOperationId();

    setupAction.InjectStylesheet("/Assets/custom-ui.css");

    setupAction.IndexStream = ()
              => typeof(Program).Assembly
              .GetManifestResourceStream("Library.API.EmbeddedAssets.index.html");


});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
