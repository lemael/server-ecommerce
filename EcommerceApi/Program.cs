using Npgsql;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.Models;
using EcommerceAPi.Services;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins(
               
                "https://client-ecommerce-eta.vercel.app/"  // Production
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Si vous utilisez des cookies
    });
});
builder.Services.AddTransient<OpenRouterService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.MapGet("/form", async () =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "add-product.html");
    var html = await File.ReadAllTextAsync(filePath);
    return Results.Content(html, "text/html");
});

app.MapPost("/add-product", async (HttpRequest request, ApplicationDbContext db) =>
{
   var form = await request.ReadFormAsync();
   var name = form["name"];
   var price = decimal.Parse(form["price"]);
   var quantity = int.Parse(form["quantity"]);
    
   if (string.IsNullOrWhiteSpace(name) || price <= 0 || quantity < 0)
    {
        return Results.BadRequest("Données invalides");
    }

    var product = new Ware
    {
        Name = name,
        Price = price,
        Quantity = quantity
    };

    db.Waren.Add(product);
    await db.SaveChangesAsync();

    return Results.Ok("Produit ajouté !");
});
app.MapGet("/waren", async (ApplicationDbContext db) =>
{
    var products = await db.Waren.ToListAsync();
    return Results.Ok(products);
});
app.MapGet("/api/products", () => new { message = "Test réussi" });
app.MapGet("/debug", async (ApplicationDbContext db) =>
{
    try
    {
        return Results.Ok(new
        {
            dbStatus = await db.Database.CanConnectAsync(),
            tables = db.Model.GetEntityTypes().Select(e => e.GetTableName())
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.ToString());
    }
});


app.UseRouting();
app.UseCors("ReactPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
