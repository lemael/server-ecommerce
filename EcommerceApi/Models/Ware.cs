// Models/Product.cs
namespace EcommerceApi.Models;

public class Ware
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
