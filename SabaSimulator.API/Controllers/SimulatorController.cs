using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SabaSimulator.API.Models.Balance;
using SabaSimulator.API.Models.CancelBet;
using SabaSimulator.API.Models.ConfirmBet;
using SabaSimulator.API.Models.ConfirmbetParlay;
using SabaSimulator.API.Models.PlaceBet;
using SabaSimulator.API.Models.PlaceBetParlay;
using SabaSimulator.API.Models.Resettle;
using SabaSimulator.API.Models.Settle;
using SabaSimulator.API.Models.Unsettle;
using Tgs.ServiceUtility;
using SabaSimulator.API.Models.SabaPlusPlaceBet;
using SabaSimulator.API.Models.BetResultCheck;
using SabaSimulator.API.Models.SabaPlusEndGame;
using SabaSimulator.API.Models.SabaPlusPlaceBetAndSettle;
using SabaSimulator.API.Models.VoidGame;
using SabaSimulator.API.Models.FreeSpin;
using SabaSimulator.API.Helper;
using SabaSimulator.API.Models.GetBetDetail;

namespace SabaSimulator.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SimulatorController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly IMemoryDb _memoryDb;
		private readonly string _baseUrl;
		private readonly string _simulatorUrl;
		private readonly string _sabaPlusUrl;

		public SimulatorController(HttpClient httpClient, IConfiguration configuration, IMemoryDb memoryDb)
		{
			_httpClient = httpClient;
			_baseUrl = configuration["BetService:BaseUrl"];
			_sabaPlusUrl = configuration["BetService:BaseSabaPlusUrl"];
			_simulatorUrl = configuration["BetService:SabaSimulatorUrl"];
			_memoryDb = memoryDb;
		}

		[HttpPost("PlaceBetParlay")]
		public async Task<PlaceBetParlayResult> PlaceBetParlay(
			[FromServices] IGetService<PlaceBetParlayRequest, PlaceBetParlayResult> service,
			PlaceBetParlayRequest source)
		{
			return await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
		}

		[HttpPost("PlaceBet")]
		public async Task<PlaceBetResult> PlaceBet(
			[FromServices] IGetService<PlaceBetRequest, PlaceBetResult> service,
			PlaceBetRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}

		[HttpPost("ConfirmBetParlay")]
		public async Task<ConfirmBetParlayResult> ConfirmBetParlay(
			[FromServices] IGetService<ConfirmBetParlayRequest, ConfirmBetParlayResult> service,
			ConfirmBetParlayRequest source)
		{

			return await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
		}

		[HttpPost("ConfirmBet")]
		public async Task<ConfirmBetResult> ConfirmBet(
		[FromServices] IGetService<ConfirmBetRequest, ConfirmBetResult> service,
		ConfirmBetRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("CancelBet")]
		public async Task<CancelBetResult> CancelBet(
			[FromServices] IGetService<CancelBetRequest, CancelBetResult> service,
			CancelBetRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

			return result;
		}

		[HttpPost("GetBetDetail")]
		public IActionResult GetBetDetail(
		[FromForm] GetBetDetailRequest source)
		{
			return Ok(_memoryDb.GetBetDetail(source));
		}

		[HttpPost("settle")]
		public async Task<SettleResult> Settle(
		[FromServices] IGetService<SettleRequest, SettleResult> service,
		SettleRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

			return result;
		}

		[HttpPost("resettle")]
		public async Task<ResettleResult> Resettle(
		[FromServices] IGetService<ResettleRequest, ResettleResult> service,
		ResettleRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

			return result;
		}

		[HttpPost("unsettle")]
		public async Task<UnsettleResult> Unsettle(
		[FromServices] IGetService<UnsettleRequest, UnsettleResult> service,
		UnsettleRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

			return result;
		}

		[HttpPost("getBalance")]
		public async Task<GetBalanceResult> GetBalance(
		[FromServices] IGetService<GetBalanceRequest, GetBalanceResult> service,
		GetBalanceRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

			return result;
		}

		[HttpPost("sabaPlus-PlaceBet")]
		public async Task<object> SabaPlusPlaceBet([FromServices] IGetService<SabaPlusPlaceBetRequest, object> service,
		SabaPlusPlaceBetRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

			return result;
		}

		[HttpPost("sabaPlus-EndGame")]
		public async Task<object> SabaPlusEndGame([FromServices] IGetService<SabaPlusEndGameRequest, object> service,
		SabaPlusEndGameRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}

		[HttpPost("sabaPlus-PlaceBetAndSettle")]
		public async Task<object> PlaceBetAndSettle([FromServices] IGetService<SabaPlusPlaceBetAndSettleRequest, object> service,
		SabaPlusPlaceBetAndSettleRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}

		[HttpPost("sabaPlus-getBalance")]
		public async Task<object> getBalance(SabaPlusGetBalanceRequest query)
		{
			try
			{
				var jsonContent = JsonSerializer.Serialize(query);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				var operatorSign = MD5Helper.ToMD5(jsonContent + query.OperatorKey);

				var request = new HttpRequestMessage(HttpMethod.Post, $"{_sabaPlusUrl}/getbalance")
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

				return responseContent;
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		[HttpPost("sabaPlus-VoidGame")]
		public async Task<object> voidGame([FromServices] IGetService<VoidGameRequest, object> service, VoidGameRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}

		[HttpPost("sabaPlus-FreeSpin")]
		public async Task<object> voidGame([FromServices] IGetService<SabaPlusFreeSpinRequest, object> service, SabaPlusFreeSpinRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}

		[HttpPost("UpdateBetResultCheck")]
		public async Task<string> BetResultCheck([FromServices] IGetService<UpdateBetResultRequest, string> service,
			UpdateBetResultRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}

		[HttpPost("GetBetResultCheckByTicketId")]
		public async Task<BetResultCheckResponse> GetResultChecks([FromServices] IGetService<GetAllBetResultChecksRequest, BetResultCheckResponse> service,
		  GetAllBetResultChecksRequest source)
		{
			var result = await service.GetAsync(source, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);
			return result;
		}
	}
}