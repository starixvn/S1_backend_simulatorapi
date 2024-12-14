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
using SabaSimulator.API.ApiClient;

namespace SabaSimulator.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SimulatorController : ControllerBase
	{
		private readonly ISabaSportApi _sabaSportApi;

		private readonly HttpClient _httpClient;
		private readonly IMemoryDb _memoryDb;
		private readonly string _baseUrl;
		private readonly string _sabaPlusSimulatorUrl;
		private readonly string _sabaSportSimulatorUrl;
		private readonly string _sabaPlusUrl;

		public SimulatorController(ISabaSportApi sabaSportApi, HttpClient httpClient, IConfiguration configuration, IMemoryDb memoryDb)
		{
			_sabaSportApi = sabaSportApi;

			_httpClient = httpClient;
			_baseUrl = configuration["BetService:BaseUrl"];
			_sabaPlusUrl = configuration["BetService:BaseSabaPlusUrl"];
			_sabaPlusSimulatorUrl = configuration["BetService:SabaPlusSimulatorUrl"];
			_sabaSportSimulatorUrl = configuration["BetService:SabaSportSimulatorUrl"];
			_memoryDb = memoryDb;
		}

		[HttpPost("PlaceBetParlay")]
		public async Task<PlaceBetParlayResult> PlaceBetParlay(PlaceBetParlayRequest source)
		{
			var result = await _sabaSportApi.PlaceBetParlay(source);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("PlaceBet")]
		public async Task<PlaceBetResult> PlaceBet(PlaceBetRequest source)
		{
			var result = await _sabaSportApi.PlaceBet(source);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("ConfirmBetParlay")]
		public async Task<ConfirmBetParlayResult> ConfirmBetParlay(ConfirmBetParlayRequest source)
		{
			var result = await _sabaSportApi.ConfirmBetParlay(source);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("ConfirmBet")]
		public async Task<ConfirmBetResult> ConfirmBet(
		[FromServices] IGetService<ConfirmBetRequest, ConfirmBetResult> service,
		ConfirmBetRequest source)
		{
			var result = await _sabaSportApi.ConfirmBet(source);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("CancelBet")]
		public async Task<CancelBetResult> CancelBet(CancelBetRequest source)
		{
			var result = await _sabaSportApi.CancelBet(source);

			return result;
		}

		[HttpPost("GetBetDetail")]
		public IActionResult GetBetDetail(
		[FromForm] GetBetDetailRequest source)
		{
			return Ok(_memoryDb.GetBetDetail(source));
		}

		[HttpPost("UpdateBetDetail")]
		public IActionResult UpdateBetDetail([FromBody] BetDetail source)
		{
			return Ok(_memoryDb.UpdateBetDetail(source));
		}

		[HttpPost("settle")]
		public async Task<SettleResult> Settle(SettleRequest source)
		{
			var result = await _sabaSportApi.Settle(source);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("resettle")]
		public async Task<ResettleResult> Resettle(ResettleRequest source)
		{
			var result = await _sabaSportApi.Resettle(source);
			_memoryDb.SetBetDetailKey(source);
			return result;
		}

		[HttpPost("unsettle")]
		public async Task<UnsettleResult> Unsettle(UnsettleRequest source)
		{
			var result = await _sabaSportApi.Unsettle(source);
			_memoryDb.SetBetDetailKey(source);

			return result;
		}

		[HttpPost("getBalance")]
		public async Task<GetBalanceResult> GetBalance(
		[FromServices] IGetService<GetBalanceRequest, GetBalanceResult> service,
		GetBalanceRequest source)
		{
			var result = await _sabaSportApi.GetBalance(source);

			return result;
		}

		[HttpPost("getSabaUrl")]
		public IActionResult GetSabaUrl([FromQuery] string vendor_member_id)
		{
			return Ok(new
			{
				error_code = 0,
				data = $"{_sabaSportSimulatorUrl}/${vendor_member_id}"
			});
		}

		[HttpPost("createMember")]
		public IActionResult CreateMember([FromForm] object data)
		{
			return Ok(new
			{
				error_code = 0,
				message = $"Simulator said Oke so It's oke now"
			});
		}

		[HttpPost("sabaPlus-PlaceBet")]
		public async Task<object> SabaPlusPlaceBet([FromServices] IGetService<SabaPlusPlaceBetRequest, object> service,
		SabaPlusPlaceBetRequest source)
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
			var result = await service.GetAsync(source, cts.Token).ConfigureAwait(false);

			return result;
		}

		[HttpPost("sabaPlus-EndGame")]
		public async Task<object> SabaPlusEndGame([FromServices] IGetService<SabaPlusEndGameRequest, object> service,
		SabaPlusEndGameRequest source)
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
			var result = await service.GetAsync(source, cts.Token).ConfigureAwait(false);

			return result;
		}

		[HttpPost("sabaPlus-PlaceBetAndSettle")]
		public async Task<object> PlaceBetAndSettle([FromServices] IGetService<SabaPlusPlaceBetAndSettleRequest, object> service,
		SabaPlusPlaceBetAndSettleRequest source)
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
			var result = await service.GetAsync(source, cts.Token).ConfigureAwait(false);

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

				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
				var response = await _httpClient.SendAsync(request, cts.Token);
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
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
			var result = await service.GetAsync(source, cts.Token).ConfigureAwait(false);

			return result;
		}

		[HttpPost("sabaPlus-FreeSpin")]
		public async Task<object> freeSpin([FromServices] IGetService<SabaPlusFreeSpinRequest, object> service, SabaPlusFreeSpinRequest source)
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
			var result = await service.GetAsync(source, cts.Token).ConfigureAwait(false);

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