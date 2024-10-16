using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.ConfirmbetParlay;

namespace SabaSimulator.API.Services;

public class ConfirmBetparlayService : IGetService<ConfirmBetParlayRequest, ConfirmBetParlayResult>
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public ConfirmBetparlayService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<ConfirmBetParlayResult> GetAsync(ConfirmBetParlayRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync($"{_baseUrl}/api/player/confirmbetparlay", content, cancellationToken);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<ConfirmBetParlayResult>(responseContent);
			// Todo

			return result;
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new ConfirmBetParlayResult("-999", 0, $"Simulator Err: {ex.Message}");
		}
	}
}