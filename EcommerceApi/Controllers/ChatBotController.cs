
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Models;
using EcommerceAPi.Services;
using EcommerceApi.Data;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/chatbot")]
public class ChatBotController : ControllerBase
{
  
    private static List<ChatExchange> _history = new List<ChatExchange>();

     private readonly OpenRouterService _openRouterService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ChatBotController> _logger;
    public ChatBotController(ILogger<ChatBotController> logger, OpenRouterService openRouterService, ApplicationDbContext db)
    {
        _openRouterService = openRouterService;
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        _logger.LogInformation("Requête reçue");
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Die Frage kann nicht leer sein.");
        }
        try
        {
              // Récupérer les 5 premiers produits en base
            var products = await _db.Waren.ToListAsync();

            // Construire la liste produits dans le prompt
            string productList = string.Join("\n", products.Select(p =>
               $"{p.Name}: Preis {p.Price}€, Menge {p.Quantity}"));

            // Construire le prompt complet avec les produits + question
          string prompt = $"Hier ist die Liste der Produkte:\n{productList}\nBeantworte die folgende Frage: {request.Question}";

            var result = await _openRouterService.GetResponseAsync(prompt);
            Console.WriteLine("Réponse finale du bot : " + result);
            Console.WriteLine("liste de produits : " + productList);
            
            return Ok(new { answer = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetHistory()
    {
        return Ok(_history);
    }
}
