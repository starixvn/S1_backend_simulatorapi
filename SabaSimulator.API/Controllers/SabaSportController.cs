using Tgs.ServiceUtility;
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
using SabaSimulator.API.Helper;
using SabaSimulator.API.Models.GetBetDetail;
using SabaSimulator.API.ApiClient;

namespace SabaSimulator.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SabaSportController(IConfiguration configuration, ISabaSportApi sabaSportApi, IMemoryDb memoryDb) : ControllerBase
	{
		private readonly string simulatorApiUrl = configuration["BetService:SabaSportSimulatorUrl"] ?? string.Empty;

		[HttpPost("ClearLoadTestData")]
		public async Task<IActionResult> ClearLoadTestData()
		{
			await memoryDb.ClearTicketsAsync();

			return Ok();
		}

		[HttpPost("BatchBetDetails")]
		public async Task<IActionResult> BatchBetDetails([FromBody] List<BetDetail> betDetails)
		{
			await memoryDb.BatchAddBetDetail(betDetails);

			return Ok(new
			{
				Message = "You done well, buddy",
			});
		}

		[HttpPost("PlaceBetParlay")]
		public async Task<PlaceBetParlayResult> PlaceBetParlay(PlaceBetParlayRequest source)
		{
			var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(30000);

			var result = await sabaSportApi.PlaceBetParlay(source, tokenSource.Token);
			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("PlaceBet")]
		public async Task<PlaceBetResult> PlaceBet(PlaceBetRequest source)
		{
			var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(30000);

			var result = await sabaSportApi.PlaceBet(source, tokenSource.Token);
			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("ConfirmBetParlay")]
		public async Task<ConfirmBetParlayResult> ConfirmBetParlay(ConfirmBetParlayRequest source)
		{
			var result = await sabaSportApi.ConfirmBetParlay(source);
			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("ConfirmBet")]
		public async Task<ConfirmBetResult> ConfirmBet(
		[FromServices] IGetService<ConfirmBetRequest, ConfirmBetResult> service,
		ConfirmBetRequest source)
		{
			var result = await sabaSportApi.ConfirmBet(source);
			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("CancelBet")]
		public async Task<CancelBetResult> CancelBet(CancelBetRequest source)
		{
			var result = await sabaSportApi.CancelBet(source);

			return result;
		}

		[HttpPost("GetBetDetail")]
		public async Task<IActionResult> GetBetDetail(
		[FromForm] GetBetDetailRequest source)
		{
			return Ok(await memoryDb.GetBetDetailAsync(source));
		}

		[HttpPost("UpdateBetDetail")]
		public async Task<IActionResult> UpdateBetDetailAsync([FromBody] BetDetail source)
		{
			return Ok(await memoryDb.UpdateBetDetailAsync(source));
		}

		[HttpPost("Settle")]
		public async Task<SettleResult> Settle(SettleRequest source)
		{
			var result = await sabaSportApi.Settle(source);

			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("Resettle")]
		public async Task<ResettleResult> Resettle(ResettleRequest source)
		{
			var result = await sabaSportApi.Resettle(source);
			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("Unsettle")]
		public async Task<UnsettleResult> Unsettle(UnsettleRequest source)
		{
			var result = await sabaSportApi.Unsettle(source);
			await memoryDb.SetBetDetailKeyAsync(source);

			return result;
		}

		[HttpPost("GetBalance")]
		public async Task<GetBalanceResult> GetBalance(
		[FromServices] IGetService<GetBalanceRequest, GetBalanceResult> service,
		GetBalanceRequest source)
		{
			var result = await sabaSportApi.GetBalance(source);

			return result;
		}

		[HttpPost("GetSabaUrl")]
		public IActionResult GetSabaUrl([FromQuery] string vendor_member_id)
		{
			var queryString = string.IsNullOrEmpty(vendor_member_id) ? string.Empty : $"/{vendor_member_id}";
			return Ok(new
			{
				error_code = 0,
				data = $"{simulatorApiUrl}{queryString}"
			});
		}

		[HttpPost("GetKenoDetailByTransID")]
		public IActionResult GetKenoDetailByTransID()
		{
			return Ok(new
			{
				error_code = 0,
				data = new
				{
					message = $"Simulator said Oke so It's oke now"
				}
			});
		}

		[HttpPost("GetBetDetailByTransID")]
		public IActionResult GetBetDetailByTransID()
		{
			return Ok(new
			{
				error_code = 0,
				data = new
				{
					message = $"Simulator said Oke so It's oke now"
				}
			});
		}

		[HttpPost("CreateMember")]
		public IActionResult CreateMember([FromForm] object data)
		{
			return Ok(new
			{
				error_code = 0,
				message = $"Simulator said Oke so It's oke now"
			});
		}
	}
}