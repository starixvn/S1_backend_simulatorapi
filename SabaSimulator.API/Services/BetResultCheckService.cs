using Microsoft.Extensions.Caching.Memory;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.BetResultCheck;

namespace SabaSimulator.API.Services;

public class BetResultCheckService : IGetService<BetResultCheckRequest, BetResultCheckResponse>
{
	private readonly IMemoryCache _memoryCache;

public BetResultCheckService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
{
	_memoryCache = memoryCache;
}

	public async ValueTask< BetResultCheckResponse> GetAsync(BetResultCheckRequest query, CancellationToken cancellationToken = default)
	{
		string key = $"{query?.Payload?.TicketNo}";
		  _memoryCache.TryGetValue(key, out BetResultCheckResponse? value);
		if ( value is not null )
		{
			value.Seq = Guid.NewGuid().ToString();
			value.TimeStamp= DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
			return value;
		}

		return new BetResultCheckResponse
		{
			Seq = Guid.NewGuid().ToString(),
			ErrorCode = 0,
			ErrorMessage = "Success",
			TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
			Payload = new BetResultPayloadModel()
			{
				OperatorCustId = query?.Payload?.OperatorCustId,
				GameRoundId="simulator-betresult-11111",
				Payout = 100,
				Status = 1,
				ActualStake = 100,
				SettleDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
				TicketNo = query?.Payload?.TicketNo,
				NetStake = 100
			}
		};
	}
}