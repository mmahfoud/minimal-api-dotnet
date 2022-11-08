using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = $"{builder.Environment.ApplicationName} v1", Version = "v1" });
    options.SwaggerDoc("v2", new() { Title = $"{builder.Environment.ApplicationName} v2", Version = "v2" });
    options.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnableTryItOutByDefault();
        options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", $"{builder.Environment.ApplicationName} v2");
    });
}

app.UseHttpsRedirection();

app.MapGet("/farewell", () => "Bye, world")
   .WithGroupName("v2");

app.MapGet("/", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = endpointSources
                .SelectMany(es => es.Endpoints)
                .OfType<RouteEndpoint>();
    var output = endpoints.Select(
        e =>
        {
            var controller = e.Metadata
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault();
            var action = controller != null
                ? $"{controller.ControllerName}.{controller.ActionName}"
                : null;
            var controllerMethod = controller != null
                ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                : null;
            return new
            {
                Method = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                Route = $"/{e.RoutePattern.RawText?.TrimStart('/')}",
                Action = action,
                ControllerMethod = controllerMethod,
                Name = e.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName,
                DisplayName = e.DisplayName,
                Description = e.Metadata.OfType<DescriptionMetadata>().FirstOrDefault()?.Description,
                Group = e.Metadata.OfType<EndpointGroupNameAttribute>().FirstOrDefault()?.EndpointGroupName,
                Tags = string.Join(", ", e.Metadata.OfType<TagsAttribute>().FirstOrDefault()?.Tags??new[] {string.Empty}),
                TypeId = e.Metadata.OfType<TagsAttribute>().FirstOrDefault()?.TypeId.ToString(),
                SwaggerSummary = e.Metadata.OfType<SwaggerOperationAttribute>().FirstOrDefault()?.Summary,
                SwaggerDescription = e.Metadata.OfType<SwaggerOperationAttribute>().FirstOrDefault()?.Description,
                SwaggerOperaionId = e.Metadata.OfType<SwaggerOperationAttribute>().FirstOrDefault()?.OperationId,
                SwaggerTags = string.Join(", ", e.Metadata.OfType<SwaggerOperationAttribute>().FirstOrDefault()?.Tags??new[] {string.Empty}),
                SwaggerTypeId = e.Metadata.OfType<SwaggerOperationAttribute>().FirstOrDefault()?.TypeId.ToString(),
            };
        }
    );

    return output;
})
.WithDisplayName("Mahfoud").WithName("Index").WithTags("Group 1")
.WithDescription("This description is added manually by Mahfoud")
.WithMetadata(new SwaggerOperationAttribute() { Summary = "Summmary X", Description = "Description X", OperationId = "NIndex", Tags=new[] {"Mahfoud"} })
.WithGroupName("v1");

app.MapGet("/greeting", () => "Hello, world").WithGroupName("v1"); ;

app.Run();

public static class Extensions
{
    public static TBuilder WithDescription<TBuilder>(this TBuilder builder, string description) where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new DescriptionMetadata() { Description = description });
        return builder;
    }
}

public class DescriptionMetadata
{
    public string? Description { get; set; }
}