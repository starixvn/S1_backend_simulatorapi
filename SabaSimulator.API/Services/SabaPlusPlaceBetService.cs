using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Helper;
using SabaSimulator.API.Models.SabaPlusPlaceBet;
using Tgs.ServiceUtility;

namespace SabaSimulator.API.Services;

public class SabaPlusPlaceBetService : IGetService<SabaPlusPlaceBetRequest,  object>
{
	private readonly HttpClient _httpClient;
	private readonly string _sabaPlusUrl;
	private readonly IMemoryCache _memoryCache;

	public SabaPlusPlaceBetService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
	{
		_httpClient = httpClient;
		_sabaPlusUrl = configuration["BetService:BaseSabaPlusUrl"];
		_memoryCache = memoryCache;
	}

	public async ValueTask<object> GetAsync(SabaPlusPlaceBetRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_sabaPlusUrl}/api/player/placebet", content);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				return errorContent.ToString();
			}
			var responseContent = await response.Content.ReadAsStringAsync();

		    var validResponse = JsonSerializer.Deserialize<SabaPlusPlaceBetResult>(responseContent);
			if ( validResponse?.ErrorCode == 0)
			{
				string key = $"{query?.Payload?.BetDataSet[0]?.TicketNo}";
				var betResultCheck = BetResultCheckBuilder.BuildBetResultByPlaceBet(query);
				_memoryCache.Set(key, betResultCheck, TimeSpan.FromMinutes(60));
			}

			return responseContent;

		}
		catch (Exception ex)
		{
			return ex;
		}
	}
}
