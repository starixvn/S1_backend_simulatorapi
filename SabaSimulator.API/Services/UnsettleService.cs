using SabaSimulator.API.Models.Settle;
using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.Unsettle;

namespace SabaSimulator.API.Services;

public class UnsettleService : IGetService<UnsettleRequest, UnsettleResult>
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public UnsettleService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<UnsettleResult> GetAsync(UnsettleRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_baseUrl}/unsettle", content, cancellationToken);
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<UnsettleResult>(responseContent);

			return result ?? new UnsettleResult("1", "Failed to deserialize response");
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new UnsettleResult("1", null);
		}
	}
}