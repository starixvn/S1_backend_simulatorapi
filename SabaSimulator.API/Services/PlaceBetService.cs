using System.Text.Json;
using System.Text;
using SabaSimulator.API.Models.PlaceBet;
using Tgs.ServiceUtility;
namespace SabaSimulator.API.Services;

public class PlaceBetService : IGetService<PlaceBetRequest, PlaceBetResult>
{

	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public PlaceBetService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<PlaceBetResult> GetAsync(PlaceBetRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_baseUrl}/api/player/placebet", content, cancellationToken);
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<PlaceBetResult>(responseContent);

			// Todo

			return result ?? new PlaceBetResult("0", null, null, "Failed to deserialize response");
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new PlaceBetResult("1", null, null, ex.Message);
		}
	}
}