using EXAMPLE.API.Data;
using Microsoft.OpenApi.Models;
using System.Reflection;
using EXAMPLE.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Adds support for JSONPatch. https://jsonpatch.com/
    options.InputFormatters.Insert(0, JsonFormatter.GetJsonPatchInputFormatter());
}).AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();

// Configure swagger.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        // Basic information about the API and who to contact.
        Version = "1.0",
        Title = "HelloID target system example API",
        Description = "This example API specifies the minimal requirements for developing a new API that will be used for user provisioning from HelloID." +
        "<br>See the Github repo linked below to download the source code.</br>",
        Contact = new OpenApiContact() { Name = "Tools4everBV Github", Url = new Uri("https://github.com/orgs/Tools4everBV/repositories/Basic-EXAMPLE-Target-API") }
    });

    // The XML is where all code comments are stored and is used to display information in the swagger interface and yaml.
    // Make sure to enable XML documentation file in project settings.
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddEntityFrameworkSqlite().AddDbContext<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(config =>
{
    // Instead of the default swagger JSON, we need the yaml file for viewing on the web.
    config.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Basic-Example-Target-API");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
