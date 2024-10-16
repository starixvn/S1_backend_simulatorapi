using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.CancelBet;

namespace SabaSimulator.API.Services;

public class CancelBetService : IGetService<CancelBetRequest, CancelBetResult>
{

	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public CancelBetService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<CancelBetResult> GetAsync(CancelBetRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync($"{_baseUrl}/api/player/cancelbet", content, cancellationToken);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<CancelBetResult>(responseContent);
			// Todo

			return result;
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new CancelBetResult("1", null, null);
		}
	}
}

