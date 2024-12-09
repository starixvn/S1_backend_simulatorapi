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

	void SetBetDetailKey(PlaceBetRequest confirmBetRequest);

	void SetBetDetailKey(ConfirmBetRequest confirmBetRequest);

	void SetBetDetailKey(ConfirmBetParlayRequest source);

	void SetBetDetailKey(ResettleRequest source);

	void SetBetDetailKey(UnsettleRequest source);

	void SetBetDetailKey(SettleRequest settleRequest);

	void ClearTickets();

	GetBetDetailResponse GetBetDetail(GetBetDetailRequest inputs);

	GetBetDetailResponse UpdateBetDetail(BetDetail betDetail);
}

public class MemoryDb() : IMemoryDb
{
	private static readonly Dictionary<string, BetDetail> BetStorage = [];

	private static long LastVersionKey = 0;
	private static bool IsUpdated = false;

	private List<BetDetail> PlacebetTicket = [];
	private List<BetDetail> ConfirmBetTicket = [];
	private List<BetDetail> SettledTicket = [];

	public void ClearTickets()
	{
		PlacebetTicket = [];
		ConfirmBetTicket = [];
		SettledTicket = [];
	}

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest request)
	{
		var confirmBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(ConfirmBetTicket)) ?? [];
		var settleBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(SettledTicket)) ?? [];

		Console.WriteLine("Request Version Key: " + request.version_key);
		Console.WriteLine("Last Version Key: " + LastVersionKey);
		Console.WriteLine("PlacebetTicket: " + PlacebetTicket.Count);
		Console.WriteLine("ConfirmBetTicket: ", ConfirmBetTicket.Count);
		Console.WriteLine("SettledTicket: ", SettledTicket);
		Console.WriteLine("TicketOnQueue: ", confirmBetTickets.Count + settleBetTickets.Count);

		if (IsUpdated && LastVersionKey <= request.version_key)
		{
			IsUpdated = false;
			LastVersionKey = request.version_key + 1;
		}

		return GenerateResponse(confirmBetTickets.Concat(settleBetTickets)
													 .Where(bet => bet.version_key >= request.version_key)
													 .GroupBy(bet => bet.trans_id)
													 .SelectMany(group => group)
													 .ToList());
	}

	private GetBetDetailResponse GenerateResponse(List<BetDetail> betDetails)
	{
		var resposne = new GetBetDetailResponse()
		{
			ErrorCode = 0,
			Message = "Success",
			Data = new()
			{
				LastVersionKey = LastVersionKey,
				BetDetails = betDetails,
				BetNumberDetails = [],
				BetVirtualSportDetails = [],
				BetCasinoDetails = [],
			}
		};

		return resposne;
	}

	public GetBetDetailResponse UpdateBetDetail(BetDetail betDetail)
	{
		var confirmBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(ConfirmBetTicket)) ?? [];
		var settleBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(SettledTicket)) ?? [];

		var ticket = ConfirmBetTicket.FirstOrDefault(x => x.trans_id == betDetail.trans_id);

		if (ticket is null)
		{
			ticket = SettledTicket.FirstOrDefault(x => x.trans_id == betDetail.trans_id);
			if (ticket is null)
			{
				return GenerateResponse(confirmBetTickets.Concat(settleBetTickets)
														 .Where(bet => bet.version_key >= LastVersionKey)
														 .GroupBy(bet => bet.trans_id)
														 .SelectMany(group => group)
														 .ToList());
			}

			betDetail.version_key = (int)LastVersionKey;
			SettledTicket.Remove(ticket);
			SettledTicket.Add(betDetail);
			IsUpdated = true;

			return GenerateResponse([SettledTicket.Find(x => x.trans_id == betDetail.trans_id)]);
		}

		betDetail.version_key = (int)LastVersionKey;
		ConfirmBetTicket.Remove(ticket);
		ConfirmBetTicket.Add(betDetail);
		IsUpdated = true;

		return GenerateResponse([ConfirmBetTicket.Find(x => x.trans_id == betDetail.trans_id)]);
	}

	public void SetBetDetailKey(SettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = ConfirmBetTicket.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			savedTicket.version_key = (int)LastVersionKey;
			savedTicket.ticket_status = transaction.Status;
			savedTicket.ticket_extra_status = transaction.ExtraStatus;
			savedTicket.settlement_time = DateTime.Parse(transaction.SettlementTime).ToUniversalTime().AddHours(-4).ToString();
			savedTicket.winlost_datetime = DateTime.Parse(transaction.WinlostDate).ToUniversalTime().AddHours(-4);
			savedTicket.winlost_amount = transaction.Payout - savedTicket.stake;

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
			savedTicket.settlement_time = DateTime.Parse(transaction.SettlementTime).ToUniversalTime().AddHours(-4).ToString();
			savedTicket.winlost_datetime = DateTime.Parse(transaction.WinlostDate).ToUniversalTime().AddHours(-4);
			savedTicket.winlost_amount = transaction.Payout - savedTicket.stake;
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
		foreach (var transaction in request.message.Txns)
		{
			var ticket = new BetDetail()
			{
				ref_id = transaction.RefId,

				operator_id = request.message.OperationId,
				vendor_member_id = request.message.UserId,

				original_stake = transaction.BetAmount,
				bet_type = 29,
				bettypename = new List<Bettypename>
				{
					new ()
					{
						lang = "en",
						name = "System Parlay"
					},
					new ()
					{
						lang = "cs",
						name = "系统串关"
					}
				}
			};
			foreach (var comboInfoDetail in transaction.Detail)
			{
				var ticketDetail = comboInfoDetail as SingleBetDetail;

				if (ticketDetail is null)
				{
					continue;
				}

				var match = request.message.TicketDetail.Find(x => x.MatchId == ticketDetail.MatchId);
				ticket.league_id = match.LeagueId;
				ticket.match_id = match.MatchId;
				ticket.home_id = match.HomeId;
				ticket.away_id = match.AwayId;
				ticket.match_datetime = match.MatchDateTime;
				ticket.sport_type = match.SportType;
				ticket.bet_type = match.BetType;
				ticket.odds = match.Odds;
				ticket.odds_type = match.OddsType;
				ticket.bet_team = match.BetTeam;
				ticket.home_score = match.HomeScore;
				ticket.away_score = match.AwayScore;
				ticket.betchoice = match.BetChoice;
				ticket.leaguename = new List<Leaguename> { new Leaguename { name = match.LeagueName, lang = "en" } };
				ticket.sportname = new List<Sportname> { new Sportname { name = match.SportTypeName, lang = "en" } };
				ticket.bettypename = new List<Bettypename> { new Bettypename { name = match.BetTypeName, lang = "en" } };
			}

			PlacebetTicket.Add(ticket);
		}
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
			transaction_time = request.message.BetTime,
			currency = request.message.Currency,
			odds_type = request.message.OddsType,
			bet_team = request.message.BetTeam,
			home_score = request.message.HomeScore,
			away_score = request.message.AwayScore,
			betfrom = request.message.BetFrom,
			betchoice = request.message.BetChoice,
			winlost_amount = request.message.Odds < 0 ? request.message.Odds * request.message.BetAmount : -request.message.BetAmount,
			leaguename = new List<Leaguename> { new Leaguename { name = request.message.LeagueName, lang = "en" } },
			sportname = new List<Sportname> { new Sportname { name = request.message.SportTypeName, lang = "en" } },
			bettypename = new List<Bettypename> { new Bettypename { name = request.message.BetTypeName, lang = "en" } },
		});
	}

	#endregion PlaceBet

	#region ConfirmBet

	public void SetBetDetailKey(ConfirmBetParlayRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			try
			{
				var ticket = PlacebetTicket.Find(x => x.ref_id == transaction.RefId);
				ticket.version_key = (int)LastVersionKey;
				ticket.trans_id = transaction.TxId;
				ticket.transaction_time = DateTime.Parse(request.message.TransactionTime).ToUniversalTime().AddHours(-4).ToString();
				ticket.ticket_status = "running";
				ConfirmBetTicket.Add(ticket);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		IsUpdated = true;
	}

	public void SetBetDetailKey(ConfirmBetRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			try
			{
				var ticket = PlacebetTicket.Find(x => x.ref_id == transaction.RefId);
				ticket.version_key = (int)LastVersionKey;
				ticket.trans_id = transaction.TxId;
				ticket.transaction_time = DateTime.Parse(request.message.TransactionTime).ToUniversalTime().AddHours(-4).ToString();
				ticket.ticket_status = "running";
				ConfirmBetTicket.Add(ticket);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		IsUpdated = true;
	}

	#endregion ConfirmBet
}