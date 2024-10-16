using SabaSimulator.API.Models.CancelBet;
using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.Balance;

namespace SabaSimulator.API.Services;

public class BalanceService : IGetService<GetBalanceRequest, GetBalanceResult>
{

	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public BalanceService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<GetBalanceResult> GetAsync(GetBalanceRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync($"{_baseUrl}/api/player/getbalance", content, cancellationToken);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<GetBalanceResult>(responseContent);
			// Todo

			return result;
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new GetBalanceResult("1", null, null, null);
		}
	}
}

