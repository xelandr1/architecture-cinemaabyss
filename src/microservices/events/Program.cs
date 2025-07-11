using Events;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://[::]:{Environment.GetEnvironmentVariable("PORT") ?? "8082"}");
builder.Logging.AddConsole();

builder.Services.AddScoped<Producer>();
builder.Services.AddHostedService<Consumer>();

var app = builder.Build();

app.MapPost("api/events/user", async context =>
{
    using var streamReader = new StreamReader(context.Request.Body);
    var body = await streamReader.ReadToEndAsync();

    var broker = context.RequestServices.GetRequiredService<Producer>();
    await broker.SendAsync(body, "user-events");

    context.Response.StatusCode = 201;
    await context.Response.WriteAsJsonAsync(new { Status = "success" });
});

app.MapPost("api/events/movie", async context =>
{
    using var streamReader = new StreamReader(context.Request.Body);
    var body = await streamReader.ReadToEndAsync();

    var broker = context.RequestServices.GetRequiredService<Producer>();
    await broker.SendAsync(body, "movie-events");

    context.Response.StatusCode = 201;
    await context.Response.WriteAsJsonAsync(new { Status = "success" });
});

app.MapPost("api/events/payment", async context =>
{
    using var streamReader = new StreamReader(context.Request.Body);
    var body = await streamReader.ReadToEndAsync();

    var broker = context.RequestServices.GetRequiredService<Producer>();
    await broker.SendAsync(body, "payment-events");

    context.Response.StatusCode = 201;
    await context.Response.WriteAsJsonAsync(new { Status = "success" });
});

app.MapGet("/api/events/health", async context =>
{
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { Status = true });
});

app.Run();