using Microsoft.Extensions.Caching.Memory;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.BetResultCheck;

namespace SabaSimulator.API.Services;

public class UpdateBetResultService : IGetService<UpdateBetResultRequest, string>
{
	private readonly IMemoryCache _memoryCache;

	public UpdateBetResultService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	public async ValueTask<string> GetAsync( UpdateBetResultRequest query, CancellationToken cancellationToken = default)
	{
		try
		{
			string key = $"{query?.TicketNo}";

			if (!string.IsNullOrEmpty(key)&& _memoryCache.TryGetValue(key, out BetResultCheckResponse? value))
			{
				_memoryCache.Set(key, query.BetResult, TimeSpan.FromMinutes(60));
				return "update bet result check success";
			}
         return "update failed"; 	    

		}
		catch (Exception ex)
		{
			return ex.ToString();
		}
	}
}
