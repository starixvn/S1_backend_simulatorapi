using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Models.BetResultCheck;
using SabaSimulator.API.Models.LaunchGame;

namespace SabaSimulator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommonWalletController(IConfiguration configuration, IMemoryCache memoryCache) : ControllerBase
{
	private readonly string simulatorUrl = configuration["BetService:SabaPlusSimulatorUrl"] ?? string.Empty;

	[HttpPost("LaunchGame")]
	public LaunchGameResult GetSimulatorURL(LaunchGameRequest request)
	{
		var res = new LaunchGameResult();
		res.Payload.ReturnUrl = $"{simulatorUrl}?userCustId={request?.Payload?.CustInfo?.OperatorCustomerId}";
		return res;
	}

	[HttpPost("BetResultCheck")]
	public IActionResult BetResultCheck(BetResultCheckRequest source)
	{
		string key = $"{source?.Payload?.TicketNo}";
		memoryCache.TryGetValue(key, out BetResultCheckResponse? value);
		if (value is not null)
		{
			value.Seq = Guid.NewGuid().ToString();
			value.TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
			return Ok(value);
		}

		return Ok(new BetResultCheckResponse
		{
			Seq = Guid.NewGuid().ToString(),
			ErrorCode = 0,
			ErrorMessage = "Success",
			TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
			Payload = new BetResultPayloadModel()
			{
				OperatorCustId = source?.Payload?.OperatorCustId,
				GameRoundId = "simulator-betresult-11111",
				Payout = 100,
				Status = 1,
				ActualStake = 100,
				SettleDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
				TicketNo = source?.Payload?.TicketNo,
				NetStake = 100
			}
		});
	}
}