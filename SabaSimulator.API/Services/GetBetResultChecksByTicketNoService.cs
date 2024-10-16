using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Models.BetResultCheck;
using Tgs.ServiceUtility;


namespace SabaSimulator.API.Services;

public class GetBetResultChecksByTicketNoService : IGetService<GetAllBetResultChecksRequest, BetResultCheckResponse>
{
	private readonly IMemoryCache _memoryCache;

	public GetBetResultChecksByTicketNoService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	public async ValueTask<BetResultCheckResponse> GetAsync(GetAllBetResultChecksRequest query, CancellationToken cancellationToken = default)
	{
		var ticketNo = query?.TicketNo;

		return _memoryCache?.Get<BetResultCheckResponse?>(ticketNo);
	}
}
