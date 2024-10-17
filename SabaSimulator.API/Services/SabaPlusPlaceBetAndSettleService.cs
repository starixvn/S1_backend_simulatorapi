using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Helper;
using SabaSimulator.API.Models.SabaPlusEndGame;
using System.Text.Json;
using System.Text;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.SabaPlusPlaceBetAndSettle;

namespace SabaSimulator.API.Services;

public class SabaPlusPlaceBetAndSettleService : IGetService<SabaPlusPlaceBetAndSettleRequest, object>
{
	private readonly HttpClient _httpClient;
	private readonly string _sabaPlusUrl;
	private readonly IMemoryCache _memoryCache;

	public SabaPlusPlaceBetAndSettleService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
	{
		_httpClient = httpClient;
		_sabaPlusUrl = configuration["BetService:BaseSabaPlusUrl"];
		_memoryCache = memoryCache;
	}

	public async ValueTask<object> GetAsync(SabaPlusPlaceBetAndSettleRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			var jsonContent = JsonSerializer.Serialize(query);
			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var operatorSign = MD5Helper.ToMD5(jsonContent + query.OperatorKey);

			var request = new HttpRequestMessage(HttpMethod.Post, $"{_sabaPlusUrl}/PlaceBetAndSettle")
			{
				Content = content
			};
			request.Headers.Add("operator-sign", operatorSign);

			var response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				return errorContent.ToString();
			}
			var responseContent = await response.Content.ReadAsStringAsync();

			var validResponse = JsonSerializer.Deserialize<SabaPlusPlaceBetAndSettleResponse>(responseContent);
			if (validResponse?.ErrorCode == 0)
			{

				var datas = BetResultCheckBuilder.BuildBetResultByPlaceBetAndSettle(query);
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
