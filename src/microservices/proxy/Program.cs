using Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath);

builder.WebHost.UseUrls("http://[::]:8000");

builder.Services.AddScoped<Router>();
builder.Services.AddHttpClient();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
}

var app = builder.Build();

app.MapGet("/Health", () => "Healthy");

app.Run(async context =>
{
    if (context.Request.Method == "GET" &&
        context.Request.Path.Value == "/health")
        return;
    
    var router = context.RequestServices.GetRequiredService<Router>();
    var content = await router.RouteRequest(context.Request);
    
    await context.Response.WriteAsync(await content.Content.ReadAsStringAsync());
});

app.Run();
