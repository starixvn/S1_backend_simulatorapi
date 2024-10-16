using SabaSimulator.API.Models.Settle;
using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.Resettle;

namespace SabaSimulator.API.Services;

public class ResettleService : IGetService<ResettleRequest, ResettleResult>
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public ResettleService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<ResettleResult> GetAsync(ResettleRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_baseUrl}/resettle", content, cancellationToken);
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<ResettleResult>(responseContent);

			// Todo

			return result ?? new ResettleResult("1", "Failed to deserialize response");
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new ResettleResult("0", null);
		}
	}
}