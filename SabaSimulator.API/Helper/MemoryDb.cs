using System.Data;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Models.ConfirmBet;
using SabaSimulator.API.Models.ConfirmbetParlay;
using SabaSimulator.API.Models.GetBetDetail;
using SabaSimulator.API.Models.Settle;

namespace SabaSimulator.API.Helper;

public interface IMemoryDb
{
	public void SetBetDetailKey(ConfirmBetRequest confirmBetRequest);

	void SetBetDetailKey(ConfirmBetParlayRequest source);

	public void SetBetDetailKey(SettleRequest settleRequest);

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest inputs);
}

public class MemoryDb(IMemoryCache memoryCache) : IMemoryDb
{
	private List<BetDetail> PlacebetTicket = new();
	private List<BetDetail> PendingTicket = new();
	private List<BetDetail> SettledTicket = new();

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest key)
	{
		var savedData = new GetBetDetailResponse()
		{
			ErrorCode = 0,
			Message = "Success"
		};

		if (PlacebetTicket.Count == 0 && SettledTicket.Count == 0)
		{
			return savedData;
		}

		if (PlacebetTicket.Count == 0)
		{
			savedData.Data = new()
			{
				LastVersionKey = 0,
				BetDetails = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(SettledTicket)) ?? [],
				BetNumberDetails = [],
				BetVirtualSportDetails = [],
				BetCasinoDetails = [],
			};

			SettledTicket = [];

			return savedData;
		}

		var tickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(PlacebetTicket)) ?? [];
		savedData.Data = new()
		{
			LastVersionKey = 0,
			BetDetails = tickets,
			BetNumberDetails = [],
			BetVirtualSportDetails = [],
			BetCasinoDetails = [],
		};

		var pendingTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(PlacebetTicket)) ?? [];
		pendingTickets.RemoveAll(ticket => SettledTicket.Exists(settleTicket => settleTicket.trans_id == ticket.trans_id));
		PendingTicket.AddRange(pendingTickets);

		PlacebetTicket = [];

		return savedData;
	}

	public void SetBetDetailKey(ConfirmBetParlayRequest request)
	{
		foreach (var transaction in request.message.Txns)
		{
			PlacebetTicket.Add(new BetDetail()
			{
				trans_id = transaction.TxId,
				vendor_member_id = request.message.UserId,
				transaction_time = request.message.TransactionTime,
				stake = transaction.ActualAmount,
				ticket_status = "running",
				odds = transaction.Odds ?? 0,
			});
		}
	}

	public void SetBetDetailKey(ConfirmBetRequest confirmBetRequest)
	{
		foreach (var transaction in confirmBetRequest.message.Txns)
		{
			PlacebetTicket.Add(new BetDetail()
			{
				trans_id = transaction.TxId,
				vendor_member_id = confirmBetRequest.message.UserId,
				transaction_time = confirmBetRequest.message.TransactionTime,
				stake = transaction.ActualAmount,
				ticket_status = "running",
				odds = transaction.Odds,
			});
		}
	}

	public void SetBetDetailKey(SettleRequest settleRequest)
	{
		var placeBetTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(PlacebetTicket)) ?? [];
		var pendingTickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(PendingTicket)) ?? [];

		placeBetTickets.AddRange(pendingTickets);
		foreach (var transaction in settleRequest.message.Txns)
		{
			var savedTicket = placeBetTickets.FindAll(ticket => ticket.trans_id.Equals(transaction.TxId));
			if (savedTicket.Count != 0)
			{
				savedTicket[0].ticket_status = transaction.Status;
				savedTicket[0].settlement_time = transaction.SettlementTime;
				savedTicket[0].winlost_amount = transaction.Payout;
				savedTicket[0].winlost_datetime = transaction.WinlostDate;
				savedTicket[0].ticket_extra_status = transaction.ExtraStatus;

				SettledTicket.Add(savedTicket[0]);

				PendingTicket.RemoveAll(ticket => ticket.trans_id == savedTicket[0].trans_id);
			}
		}
	}
}