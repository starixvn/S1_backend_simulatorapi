using System.Text.Json;
using SabaSimulator.API.Models.ConfirmBet;
using SabaSimulator.API.Models.ConfirmbetParlay;
using SabaSimulator.API.Models.GetBetDetail;
using SabaSimulator.API.Models.PlaceBet;
using SabaSimulator.API.Models.PlaceBetParlay;
using SabaSimulator.API.Models.Resettle;
using SabaSimulator.API.Models.Settle;
using SabaSimulator.API.Models.Unsettle;

namespace SabaSimulator.API.Helper;

public interface IMemoryDb
{
	void SetBetDetailKey(PlaceBetParlayRequest source);

	public void SetBetDetailKey(PlaceBetRequest confirmBetRequest);

	public void SetBetDetailKey(ConfirmBetRequest confirmBetRequest);

	void SetBetDetailKey(ConfirmBetParlayRequest source);

	void SetBetDetailKey(ResettleRequest source);

	void SetBetDetailKey(UnsettleRequest source);

	public void SetBetDetailKey(SettleRequest settleRequest);

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest inputs);
}

public class MemoryDb() : IMemoryDb
{
	private static readonly Dictionary<string, BetDetail> BetStorage = [];

	private static long LastVersionKey = 0;
	private static bool IsUpdated = false;

	private List<BetDetail> PlacebetTicket = new();
	private List<BetDetail> ConfirmBetTicket = new();
	private List<BetDetail> SettledTicket = new();

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest key)
	{
		LastVersionKey = key.version_key;

		var confirmBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(ConfirmBetTicket)) ?? [];
		var settleBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(SettledTicket)) ?? [];

		return GenerateRespone(confirmBetTickets.Concat(settleBetTickets)
													 .Where(bet => bet.version_key >= LastVersionKey)
													 .GroupBy(bet => bet.trans_id)
													 .SelectMany(group => group)
													 .ToList());
	}

	private GetBetDetailResponse GenerateRespone(List<BetDetail> betDetails)
	{
		if (IsUpdated)
		{
			IsUpdated = false;
			LastVersionKey = LastVersionKey + 1;
		}

		var resposne = new GetBetDetailResponse()
		{
			ErrorCode = 0,
			Message = "Success"
		};

		resposne.Data = new()
		{
			LastVersionKey = LastVersionKey,
			BetDetails = betDetails,
			BetNumberDetails = [],
			BetVirtualSportDetails = [],
			BetCasinoDetails = [],
		};

		return resposne;
	}

	public void SetBetDetailKey(SettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = ConfirmBetTicket.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			savedTicket.version_key = (int)LastVersionKey;
			savedTicket.ticket_status = transaction.Status;
			savedTicket.ticket_extra_status = transaction.ExtraStatus;
			savedTicket.settlement_time = DateTime.Parse(transaction.SettlementTime).ToUniversalTime().ToString();
			savedTicket.winlost_amount = transaction.Payout;
			savedTicket.winlost_datetime = transaction.WinlostDate;

			SettledTicket.Add(savedTicket);
			ConfirmBetTicket.Remove(savedTicket);
		}
		IsUpdated = true;
	}

	public void SetBetDetailKey(ResettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = SettledTicket.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			savedTicket.version_key = (int)LastVersionKey;
			savedTicket.ticket_status = transaction.Status;
			savedTicket.ticket_extra_status = transaction.ExtraStatus;
			savedTicket.settlement_time = DateTime.Parse(transaction.SettlementTime).ToUniversalTime().ToString();
			savedTicket.winlost_amount = transaction.Payout;
			savedTicket.winlost_datetime = string.IsNullOrEmpty(transaction.WinlostDate) ? string.Empty : transaction.WinlostDate;
		}
		IsUpdated = true;
	}

	public void SetBetDetailKey(UnsettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = SettledTicket.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			SettledTicket.Remove(savedTicket);

			savedTicket.version_key = (int)LastVersionKey;
			ConfirmBetTicket.Add(savedTicket);
		}
		IsUpdated = true;
	}

	#region PlaceBet

	public void SetBetDetailKey(PlaceBetParlayRequest request)
	{
		//foreach(var transaction in request.message.Txns)
		//{
		//	PlacebetTicket.Add(new BetDetail()
		//	{
		//		ref_id = request.message.RefId,

		//		operator_id = request.message.OperationId,
		//		vendor_member_id = request.message.UserId,
		//		league_id = request.message.LeagueId,
		//		match_id = request.message.MatchId,
		//		home_id = request.message.HomeId,
		//		away_id = request.message.AwayId,
		//		match_datetime = request.message.MatchDateTime,
		//		sport_type = request.message.SportType,
		//		bet_type = request.message.BetType,
		//		odds = request.message.Odds,
		//		stake = request.message.BetAmount,
		//		discount_stake = request.message.ActualAmount,
		//		transaction_time = request.message.BetTime,
		//		currency = request.message.Currency,
		//		odds_type = request.message.OddsType,
		//		bet_team = request.message.BetTeam,
		//		home_score = request.message.HomeScore,
		//		away_score = request.message.AwayScore,
		//		betfrom = request.message.BetFrom,
		//		betchoice = request.message.BetChoice,
		//		original_stake = request.message.BetAmount,
		//		winlost_datetime = string.Empty,
		//		leaguename = new List<Leaguename> { new Leaguename { name = request.message.LeagueName } },
		//		sportname = new List<Sportname> { new Sportname { name = request.message.SportTypeName } },
		//		bettypename = new List<Bettypename> { new Bettypename { name = request.message.BetTypeName } },
		//	});
		//}
	}

	public void SetBetDetailKey(PlaceBetRequest request)
	{
		PlacebetTicket.Add(new BetDetail()
		{
			ref_id = request.message.RefId,

			operator_id = request.message.OperationId,
			vendor_member_id = request.message.UserId,
			league_id = request.message.LeagueId,
			match_id = request.message.MatchId,
			home_id = request.message.HomeId,
			away_id = request.message.AwayId,
			match_datetime = request.message.MatchDateTime,
			sport_type = request.message.SportType,
			bet_type = request.message.BetType,
			odds = request.message.Odds,
			stake = request.message.BetAmount,
			discount_stake = request.message.ActualAmount,
			transaction_time = request.message.BetTime,
			currency = request.message.Currency,
			odds_type = request.message.OddsType,
			bet_team = request.message.BetTeam,
			home_score = request.message.HomeScore,
			away_score = request.message.AwayScore,
			betfrom = request.message.BetFrom,
			betchoice = request.message.BetChoice,
			original_stake = request.message.BetAmount,
			winlost_datetime = string.Empty,
			leaguename = new List<Leaguename> { new Leaguename { name = request.message.LeagueName } },
			sportname = new List<Sportname> { new Sportname { name = request.message.SportTypeName } },
			bettypename = new List<Bettypename> { new Bettypename { name = request.message.BetTypeName } },
		});
	}

	#endregion PlaceBet

	#region ConfirmBet

	public void SetBetDetailKey(ConfirmBetParlayRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var ticket = PlacebetTicket.Find(x => x.ref_id == transaction.RefId);
			ticket.version_key = (int)LastVersionKey;
			ticket.trans_id = transaction.TxId;
			ticket.vendor_member_id = request.message.UserId;
			ticket.transaction_time = DateTime.Parse(request.message.TransactionTime).ToUniversalTime().ToString();
			ticket.stake = transaction.ActualAmount;
			ticket.ticket_status = "running";
			ticket.odds = transaction.Odds ?? 0;
			ticket.winlost_datetime = transaction.WinlostDate;
			ConfirmBetTicket.Add(ticket);
		}

		IsUpdated = true;
	}

	public void SetBetDetailKey(ConfirmBetRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var ticket = PlacebetTicket.Find(x => x.ref_id == transaction.RefId);
			ticket.version_key = (int)LastVersionKey;
			ticket.trans_id = transaction.TxId;
			ticket.vendor_member_id = request.message.UserId;
			ticket.transaction_time = DateTime.Parse(request.message.TransactionTime).ToUniversalTime().ToString();
			ticket.stake = transaction.ActualAmount;
			ticket.ticket_status = "running";
			ticket.odds = transaction.Odds;
			ticket.winlost_datetime = DateTime.UnixEpoch.ToString();
			ConfirmBetTicket.Add(ticket);
		}

		IsUpdated = true;
	}

	#endregion ConfirmBet
}