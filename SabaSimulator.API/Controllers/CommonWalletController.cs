using Microsoft.AspNetCore.Mvc;
using SabaSimulator.API.Models.BetResultCheck;
using SabaSimulator.API.Models.LaunchGame;
using Tgs.ServiceUtility;

namespace SabaSimulator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommonWalletController(IConfiguration configuration) : ControllerBase
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
	public async Task<BetResultCheckResponse> BetResultCheck([FromServices] IGetService<BetResultCheckRequest, BetResultCheckResponse> service,
	 BetResultCheckRequest source)
	{
		var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
		return result;
	}
}