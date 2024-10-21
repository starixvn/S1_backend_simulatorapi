using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using SabaSimulator.API.Models.ConfirmBet;
using SabaSimulator.API.Models.GetBetDetail;
using SabaSimulator.API.Models.Settle;

namespace SabaSimulator.API.Helper;

public interface IMemoryDb
{
	public void SetBetDetailKey(ConfirmBetRequest confirmBetRequest);

	public void SetBetDetailKey(SettleRequest settleRequest);

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest inputs);
}

public class MemoryDb(IMemoryCache memoryCache) : IMemoryDb
{
	private List<BetDetail> PlacebetTicket = new List<BetDetail>();
	private List<BetDetail> SettledTicket = new List<BetDetail>();

	public GetBetDetailResponse GetBetDetail(GetBetDetailRequest key)
	{
		var savedData = new GetBetDetailResponse()
		{
			ErrorCode = 0,
			Message = "Success"
		};

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

		SettledTicket.AddRange(tickets.Where(ticket => !SettledTicket.Exists(x => x.trans_id.Equals(ticket))));

		PlacebetTicket = [];

		return savedData;
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
			});
		}
	}

	public void SetBetDetailKey(SettleRequest settleRequest)
	{
		var tickets = JsonSerializer.Deserialize<List<BetDetail>>(JsonSerializer.Serialize(PlacebetTicket)) ?? [];
		foreach (var transaction in settleRequest.message.Txns)
		{
			var savedTicket = tickets.FindAll(ticket => ticket.trans_id.Equals(transaction.TxId));
			if (savedTicket.Count != 0)
			{
				SettledTicket.Add(savedTicket[0]);
			}
		}
	}
}