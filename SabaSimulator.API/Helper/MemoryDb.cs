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
	Task SetBetDetailKeyAsync(PlaceBetParlayRequest source);

	Task SetBetDetailKeyAsync(PlaceBetRequest confirmBetRequest);

	Task SetBetDetailKeyAsync(ConfirmBetRequest confirmBetRequest);

	Task SetBetDetailKeyAsync(ConfirmBetParlayRequest source);

	Task SetBetDetailKeyAsync(ResettleRequest source);

	Task SetBetDetailKeyAsync(UnsettleRequest source);

	Task SetBetDetailKeyAsync(SettleRequest settleRequest);

	Task BatchAddBetDetail(List<BetDetail> betDetails);

	Task ClearTicketsAsync();

	Task<GetBetDetailResponse> GetBetDetailAsync(GetBetDetailRequest inputs);

	Task<GetBetDetailResponse> UpdateBetDetailAsync(BetDetail betDetail);
}

public class MemoryDb : IMemoryDb
{
	private static readonly Dictionary<string, BetDetail> BetStorage = [];

	private static long LastVersionKey = 0;
	private static bool IsUpdated = false;

	private ConcurrentList<BetDetail> _placebetTickets = new();
	private ConcurrentList<BetDetail> _confirmBetTickets = new();
	private ConcurrentList<BetDetail> _settledTickets = new();
	private List<BetDetail> _loadTestTickets = new();

	public async Task ClearTicketsAsync()
	{
		await _placebetTickets.Clear();
		await _confirmBetTickets.Clear();
		await _settledTickets.Clear();
	}

	public async Task<GetBetDetailResponse> GetBetDetailAsync(GetBetDetailRequest request)
	{
		var confirmBetTickets = await _confirmBetTickets.GetAll();
		var settleBetTickets = await _settledTickets.GetAll();

		Console.WriteLine("Request Version Key: " + request.version_key);
		Console.WriteLine("Last Version Key: " + LastVersionKey);
		Console.WriteLine("PlacebetTicket: " + _placebetTickets.Count);
		Console.WriteLine("ConfirmBetTicket: ", _confirmBetTickets.Count);
		Console.WriteLine("SettledTicket: ", _settledTickets);
		Console.WriteLine("TicketOnQueue: ", confirmBetTickets.Count + settleBetTickets.Count);

		if (IsUpdated && LastVersionKey <= request.version_key)
		{
			IsUpdated = false;
			LastVersionKey = request.version_key + 1;
		}

		return GenerateResponse(confirmBetTickets.Concat(settleBetTickets).Concat(_loadTestTickets)
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

	public async Task BatchAddBetDetail(List<BetDetail> betDetails)
	{
		foreach (var bet in betDetails)
		{
			bet.version_key = (int)LastVersionKey;
		}

		_loadTestTickets.AddRange(betDetails);
		IsUpdated = true;
	}

	public async Task<GetBetDetailResponse> UpdateBetDetailAsync(BetDetail betDetail)
	{
		var confirmBetTickets = await _confirmBetTickets.GetAll();
		var settleBetTickets = await _settledTickets.GetAll();

		var ticket = await _confirmBetTickets.FirstOrDefault(x => x.trans_id == betDetail.trans_id);

		if (ticket is null)
		{
			ticket = await _settledTickets.FirstOrDefault(x => x.trans_id == betDetail.trans_id);
			if (ticket is null)
			{
				return GenerateResponse(confirmBetTickets.Concat(settleBetTickets)
														 .Where(bet => bet.version_key >= LastVersionKey)
														 .GroupBy(bet => bet.trans_id)
														 .SelectMany(group => group)
														 .ToList());
			}

			betDetail.version_key = (int)LastVersionKey;
			await _settledTickets.Remove(ticket);
			await _settledTickets.Add(betDetail);
			IsUpdated = true;

			return GenerateResponse([await _settledTickets.Find(x => x.trans_id == betDetail.trans_id)]);
		}

		betDetail.version_key = (int)LastVersionKey;
		await _confirmBetTickets.Remove(ticket);
		await _confirmBetTickets.Add(betDetail);
		IsUpdated = true;

		return GenerateResponse([await _confirmBetTickets.Find(x => x.trans_id == betDetail.trans_id)]);
	}

	public async Task SetBetDetailKeyAsync(SettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = await _confirmBetTickets.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			savedTicket.version_key = (int)LastVersionKey;
			savedTicket.ticket_status = transaction.Status;
			savedTicket.ticket_extra_status = transaction.ExtraStatus;
			savedTicket.settlement_time = DateTime.Parse(transaction.SettlementTime).ToUniversalTime().AddHours(-4).ToString();
			savedTicket.winlost_datetime = DateTime.Parse(transaction.WinlostDate).ToUniversalTime().AddHours(-4);
			savedTicket.winlost_amount = transaction.Payout - savedTicket.stake;

			await _settledTickets.Add(savedTicket);
			await _confirmBetTickets.Remove(savedTicket);
		}
		IsUpdated = true;
	}

	public async Task SetBetDetailKeyAsync(ResettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = await _settledTickets.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			savedTicket.version_key = (int)LastVersionKey;
			savedTicket.ticket_status = transaction.Status;
			savedTicket.ticket_extra_status = transaction.ExtraStatus;
			savedTicket.settlement_time = DateTime.Parse(transaction.SettlementTime).ToUniversalTime().AddHours(-4).ToString();
			savedTicket.winlost_datetime = DateTime.Parse(transaction.WinlostDate).ToUniversalTime().AddHours(-4);
			savedTicket.winlost_amount = transaction.Payout - savedTicket.stake;
		}
		IsUpdated = true;
	}

	public async Task SetBetDetailKeyAsync(UnsettleRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			var savedTicket = await _settledTickets.Find(ticket => ticket.trans_id.Equals(transaction.TxId));
			await _settledTickets.Remove(savedTicket);

			savedTicket.version_key = (int)LastVersionKey;
			await _confirmBetTickets.Add(savedTicket);
		}
		IsUpdated = true;
	}

	#region PlaceBet

	public async Task SetBetDetailKeyAsync(PlaceBetParlayRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			foreach (var comboInfoDetail in transaction.Detail)
			{
				var ticket = new BetDetail()
				{
					//TODO: Double check later
					operator_id = "Starix1SW",
					winlost_amount = -transaction.BetAmount,
					ref_id = transaction.RefId,
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

				var singleBetDetail = comboInfoDetail as SingleBetDetail;

				if (singleBetDetail is not null)
				{
					ticket.stake = transaction.BetAmount;

					var match = request.message.TicketDetail.Find(x => x.MatchId == singleBetDetail.MatchId);
					ticket.league_id = match.LeagueId;
					ticket.match_id = match.MatchId;
					ticket.home_id = match.HomeId;
					ticket.away_id = match.AwayId;
					ticket.match_datetime = match.MatchDateTime;
					ticket.sport_type = match.SportType;
					ticket.bet_type = match.BetType;
					ticket.odds_type = match.OddsType;
					ticket.bet_team = match.BetTeam;
					ticket.home_score = match.HomeScore;
					ticket.away_score = match.AwayScore;
					ticket.betchoice = match.BetChoice;
					ticket.leaguename = [new Leaguename { name = match.LeagueName, lang = "en" }];
					ticket.sportname = [new Sportname { name = match.SportTypeName, lang = "en" }];
					ticket.bettypename = [new Bettypename { name = match.BetTypeName, lang = "en" }];
					await _placebetTickets.Add(ticket);
					continue;
				}

				var parlayBetDetail = comboInfoDetail as ParlayDetail;
				if (parlayBetDetail != null)
				{
					ticket.odds = parlayBetDetail.Odds ?? 0;
					ticket.stake = parlayBetDetail.Stake;
					ticket.bet_team = parlayBetDetail.Type.ToString();
					await _placebetTickets.Add(ticket);
					continue;
				}
			}
		}
	}

	public async Task SetBetDetailKeyAsync(PlaceBetRequest request)
	{
		await _placebetTickets.Add(new BetDetail()
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

	public async Task SetBetDetailKeyAsync(ConfirmBetParlayRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			try
			{
				var ticket = await _placebetTickets.Find(x => x.ref_id == transaction.RefId);
				ticket.version_key = (int)LastVersionKey;
				ticket.trans_id = transaction.TxId;
				ticket.transaction_time = DateTime.Parse(request.message.TransactionTime).ToUniversalTime().AddHours(-4).ToString();
				ticket.ticket_status = "running";
				await _confirmBetTickets.Add(ticket);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		IsUpdated = true;
	}

	public async Task SetBetDetailKeyAsync(ConfirmBetRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			try
			{
				var ticket = await _placebetTickets.Find(x => x.ref_id == transaction.RefId);
				ticket.version_key = (int)LastVersionKey;
				ticket.trans_id = transaction.TxId;
				ticket.transaction_time = DateTime.Parse(request.message.TransactionTime).ToUniversalTime().AddHours(-4).ToString();
				ticket.ticket_status = "running";
				await _confirmBetTickets.Add(ticket);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		IsUpdated = true;
	}

	#endregion ConfirmBet
}