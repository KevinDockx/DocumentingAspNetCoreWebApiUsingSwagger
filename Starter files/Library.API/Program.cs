using Library.API.Contexts;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(configure =>
{
    configure.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson(setupAction =>
{
    setupAction.SerializerSettings.ContractResolver =
       new CamelCasePropertyNamesContractResolver();
});

// configure the NewtonsoftJsonOutputFormatter
builder.Services.Configure<MvcOptions>(configureOptions => 
{
    var jsonOutputFormatter = configureOptions.OutputFormatters
        .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

    // remove text/json as it isn't the approved media type
    // for working with JSON at API level
    jsonOutputFormatter?.SupportedMediaTypes.Remove("text/json");
}); 

builder.Services.AddDbContext<LibraryContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:LibraryDBConnectionString"]));

builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
