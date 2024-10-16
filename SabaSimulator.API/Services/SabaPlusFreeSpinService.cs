using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Helper;
using SabaSimulator.API.Models.FreeSpin;
using SabaSimulator.API.Models.SabaPlusPlaceBetAndSettle;
using Tgs.ServiceUtility;

namespace SabaSimulator.API.Services;

public class SabaPlusFreeSpinService : IGetService<SabaPlusFreeSpinRequest, object>
{
	private readonly HttpClient _httpClient;
	private readonly string _sabaPlusUrl;
	private readonly IMemoryCache _memoryCache;

	public SabaPlusFreeSpinService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
	{
		_httpClient = httpClient;
		_sabaPlusUrl = configuration["BetService:BaseSabaPlusUrl"];
		_memoryCache = memoryCache;
	}

	public async ValueTask<object> GetAsync(SabaPlusFreeSpinRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"{_sabaPlusUrl}/api/player/FreeSpin", content);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				return errorContent.ToString();
			}
			var responseContent = await response.Content.ReadAsStringAsync();

			var validResponse = JsonSerializer.Deserialize<SabaPlusPlaceBetAndSettleResponse>(responseContent);
			if (validResponse?.ErrorCode == 0)
			{
				var datas = BetResultCheckBuilder.BuildBetResultByFreeSpin(query);
				foreach (var ticket in datas)
				{
					string key = $"{ticket.Payload.TicketNo}";
					_memoryCache.Set(key, ticket, TimeSpan.FromMinutes(60));
				}
			}

			return responseContent;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}
}