using Microsoft.AspNetCore.Mvc;
using SabaSimulator.API.Models.Balance;
using SabaSimulator.API.Models.SabaPlusPlaceBet;
using SabaSimulator.API.Models.SabaPlusEndGame;
using SabaSimulator.API.Models.SabaPlusPlaceBetAndSettle;
using SabaSimulator.API.Models.VoidGame;
using SabaSimulator.API.Models.FreeSpin;
using SabaSimulator.API.ApiClient;
using SabaSimulator.API.Helper;
using Microsoft.Extensions.Caching.Memory;

namespace SabaSimulator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SabaPlusController(ISabaPlusApi sabaPlusApi, IMemoryCache memoryCache) : ControllerBase
{
	[HttpPost("PlaceBet")]
	public async Task<object> SabaPlusPlaceBet(SabaPlusPlaceBetRequest source)
	{
		var response = await sabaPlusApi.PlaceBet(source);
		if (response.ErrorCode == 0)
		{
			string key = $"{source?.Payload?.BetDataSet[0]?.TicketNo}";
			var betResultCheck = BetResultCheckBuilder.BuildBetResultByPlaceBet(source!);
			memoryCache.Set(key, betResultCheck, TimeSpan.FromMinutes(60));
		}

		return response;
	}

	[HttpPost("EndGame")]
	public async Task<object> SabaPlusEndGame(SabaPlusEndGameRequest source)
	{
		var response = await sabaPlusApi.EndGame(source);
		if (response.ErrorCode == 0)
		{
			var datas = BetResultCheckBuilder.BuildBetResultBySettle(source);
			foreach (var ticket in datas)
			{
				string key = $"{ticket?.Payload?.TicketNo}";
				memoryCache.Set(key, ticket, TimeSpan.FromMinutes(60));
			}
		}

		return response!;
	}

	[HttpPost("PlaceBetAndSettle")]
	public async Task<object> PlaceBetAndSettle(SabaPlusPlaceBetAndSettleRequest source)
	{
		var response = await sabaPlusApi.PlaceBetAndSettle(source);
		if (response.ErrorCode == 0)
		{
			var datas = BetResultCheckBuilder.BuildBetResultByPlaceBetAndSettle(source);
			foreach (var ticket in datas)
			{
				string key = $"{ticket?.Payload?.TicketNo}";
				memoryCache.Set(key, ticket, TimeSpan.FromMinutes(60));
			}
		}
		return response!;
	}

	[HttpPost("GetBalance")]
	public async Task<object> GetBalance(SabaPlusGetBalanceRequest source)
	{
		var response = await sabaPlusApi.GetBalance(source);

		return response;
	}

	[HttpPost("VoidGame")]
	public async Task<object> VoidGame(VoidGameRequest source)
	{
		var response = await sabaPlusApi.VoidGame(source);
		if (response.ErrorCode == 0)
		{
			var datas = BetResultCheckBuilder.BuildBetResultByVoidGame(source);
			foreach (var ticket in datas)
			{
				string key = $"{ticket?.Payload?.TicketNo}";
				memoryCache.Set(key, ticket, TimeSpan.FromMinutes(60));
			}
		}

		return response;
	}

	[HttpPost("FreeSpin")]
	public async Task<object> FreeSpin(SabaPlusFreeSpinRequest source)
	{
		var response = await sabaPlusApi.FreeSpin(source);
		if (response.ErrorCode == 0)
		{
			var datas = BetResultCheckBuilder.BuildBetResultByFreeSpin(source);
			foreach (var ticket in datas)
			{
				string key = $"{ticket?.Payload?.TicketNo}";
				memoryCache.Set(key, ticket, TimeSpan.FromMinutes(60));
			}
		}

		return response;
	}

	//[HttpPost("UpdateBetResultCheck")]
	//public async Task<string> BetResultCheck([FromServices] IGetService<UpdateBetResultRequest, string> service,
	//	UpdateBetResultRequest source)
	//{
	//	var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
	//	return result;
	//}

	//[HttpPost("GetBetResultCheckByTicketId")]
	//public async Task<BetResultCheckResponse> GetResultChecks([FromServices] IGetService<GetAllBetResultChecksRequest, BetResultCheckResponse> service, GetAllBetResultChecksRequest source)
	//{
	//	var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

	//	return result;
	//}
}