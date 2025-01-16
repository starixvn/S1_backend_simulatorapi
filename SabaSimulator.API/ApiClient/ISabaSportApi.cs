using Refit;
using SabaSimulator.API.Models.Balance;
using SabaSimulator.API.Models.CancelBet;
using SabaSimulator.API.Models.ConfirmBet;
using SabaSimulator.API.Models.ConfirmbetParlay;
using SabaSimulator.API.Models.PlaceBet;
using SabaSimulator.API.Models.PlaceBetParlay;
using SabaSimulator.API.Models.Resettle;
using SabaSimulator.API.Models.Settle;
using SabaSimulator.API.Models.Unsettle;

namespace SabaSimulator.API.ApiClient;

public interface ISabaSportApi
{
	[Post("/PlaceBetParlay")]
	Task<PlaceBetParlayResult> PlaceBetParlay(PlaceBetParlayRequest req);

	[Post("/PlaceBet")]
	Task<PlaceBetResult> PlaceBet(PlaceBetRequest req);

	[Post("/ConfirmBetParlay")]
	Task<ConfirmBetParlayResult> ConfirmBetParlay(ConfirmBetParlayRequest req);

	[Post("/ConfirmBet")]
	Task<ConfirmBetResult> ConfirmBet(ConfirmBetRequest req);

	[Post("/CancelBet")]
	Task<CancelBetResult> CancelBet(CancelBetRequest req);

	[Post("/Settle")]
	Task<SettleResult> Settle(SettleRequest req);

	[Post("/Resettle")]
	Task<ResettleResult> Resettle(ResettleRequest req);

	[Post("/Unsettle")]
	Task<UnsettleResult> Unsettle(UnsettleRequest req);

	[Post("/GetBalance")]
	Task<GetBalanceResult> GetBalance(GetBalanceRequest req);
}