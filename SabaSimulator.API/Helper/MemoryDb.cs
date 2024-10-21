using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Models.ConfirmBet;
using SabaSimulator.API.Models.GetBetDetail;

namespace SabaSimulator.API.Helper;

public interface IMemoryDb
{
	public void SetBetDetailKey(ConfirmBetRequest confirmBetRequest);

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest inputs);
}

public class MemoryDb(IMemoryCache memoryCache) : IMemoryDb
{
	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest key)
	{
		var savedData = new GetBetDetailResponse()
		{
			ErrorCode = 0,
			Message = "Success"
		};
		var exist = memoryCache.TryGetValue(key.vendor_id, out List<BetDetail>? betDetails);

		if (!exist || betDetails is null)
		{
			savedData.Data = new()
			{
				BetDetails = [],
				BetNumberDetails = [],
				BetVirtualSportDetails = [],
				BetCasinoDetails = [],
			};
			return savedData;
		}

		memoryCache.Set<GetBetDetailResponse?>(key.vendor_id, default);

		savedData.Data = new()
		{
			LastVersionKey = 0,
			BetDetails = betDetails,
			BetNumberDetails = [],
			BetVirtualSportDetails = [],
			BetCasinoDetails = [],
		};
		return savedData;
	}

	public void SetBetDetailKey(ConfirmBetRequest confirmBetRequest)
	{
		var exist = memoryCache.TryGetValue("y2g53zl9g4", out List<BetDetail>? betDetails);

		betDetails = betDetails ?? [];

		foreach (var transaction in confirmBetRequest.message.Txns)
		{
			betDetails.Add(new BetDetail()
			{
				trans_id = transaction.TxId,
				vendor_member_id = confirmBetRequest.message.UserId,
				transaction_time = confirmBetRequest.message.TransactionTime,
			});
		}

		memoryCache.Set("y2g53zl9g4", betDetails);
	}
}