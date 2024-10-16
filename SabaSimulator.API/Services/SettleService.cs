using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.Settle;

namespace SabaSimulator.API.Services;

public class SettleService : IGetService<SettleRequest, SettleResult>
{

	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public SettleService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<SettleResult> GetAsync(SettleRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_baseUrl}/api/player/settle", content, cancellationToken);
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<SettleResult>(responseContent);

			// Todo

			return result ?? new SettleResult("1",  "Failed to deserialize response");
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new SettleResult("0",  null);
		}
	}
}
