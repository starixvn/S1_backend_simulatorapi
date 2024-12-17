using Refit;
using SabaSimulator.API.Models.Balance;
using SabaSimulator.API.Models.FreeSpin;
using SabaSimulator.API.Models.SabaPlusEndGame;
using SabaSimulator.API.Models.SabaPlusPlaceBet;
using SabaSimulator.API.Models.SabaPlusPlaceBetAndSettle;
using SabaSimulator.API.Models.VoidGame;

namespace SabaSimulator.API.ApiClient;

public interface ISabaPlusApi
{
	[Post("/PlaceBet")]
	Task<SabaPlusPlaceBetResult> PlaceBet(SabaPlusPlaceBetRequest req);

	[Post("/EndGame")]
	Task<SabaPlusEndGameResult> EndGame(SabaPlusEndGameRequest req);

	[Post("/PlaceBetAndSettle")]
	Task<SabaPlusPlaceBetAndSettleResponse> PlaceBetAndSettle(SabaPlusPlaceBetAndSettleRequest req);

	[Post("/GetBalance")]
	Task<GetBalanceResult> GetBalance(SabaPlusGetBalanceRequest req);

	[Post("/VoidGame")]
	Task<VoidGameResult> VoidGame(VoidGameRequest req);

	[Post("/FreeSpin")]
	Task<SabaPlusFreeSpinResponse> FreeSpin(SabaPlusFreeSpinRequest req);
}