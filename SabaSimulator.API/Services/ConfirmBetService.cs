using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.ConfirmBet;

namespace SabaSimulator.API.Services;

public class ConfirmBetService : IGetService<ConfirmBetRequest, ConfirmBetResult>
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public ConfirmBetService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["BetService:BaseUrl"];
	}

	public async ValueTask<ConfirmBetResult> GetAsync(ConfirmBetRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync($"{_baseUrl}/confirmbet", content, cancellationToken);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			var result = JsonSerializer.Deserialize<ConfirmBetResult>(responseContent);
			// Todo

			return result;
		}
		catch (Exception ex)
		{
			Console.Write(ex.ToString());
			return new ConfirmBetResult("1", null, ex.ToString());
		}
	}
}