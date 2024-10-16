using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.PlaceBetParlay;

namespace SabaSimulator.API.Services;

public class PlaceBetParlayService : IGetService<PlaceBetParlayRequest, PlaceBetParlayResult>
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public PlaceBetParlayService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<PlaceBetParlayResult> GetAsync(PlaceBetParlayRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_baseUrl}/api/player/placebetparlay", content, cancellationToken);
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<PlaceBetParlayResult>(responseContent);

			// Todo

			return result;
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new PlaceBetParlayResult("-999", null, $"Simulator Err: {ex.Message}");
		}
	}
}