using SabaSimulator.API.Models.BetResultCheck;
using SabaSimulator.API.Models.FreeSpin;
using SabaSimulator.API.Models.SabaPlusEndGame;
using SabaSimulator.API.Models.SabaPlusPlaceBet;
using SabaSimulator.API.Models.SabaPlusPlaceBetAndSettle;
using SabaSimulator.API.Models.VoidGame;

namespace SabaSimulator.API.Helper;

public static class BetResultCheckBuilder
{
	public static BetResultCheckResponse BuildBetResultByPlaceBet(SabaPlusPlaceBetRequest data)
		=> new BetResultCheckResponse
		{
			ErrorCode = 0,
			ErrorMessage = "Success",
			Seq = Guid.NewGuid().ToString(),
			TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
			Payload = new BetResultPayloadModel
			{
				TicketNo = data?.Payload?.BetDataSet[0]?.TicketNo,
				OperatorCustId = data?.Payload?.OperatorCustId,
				GameRoundId = data?.Payload?.BetDataSet[0].GameRoundId,
				TransactionId = data?.Payload?.BetDataSet[0].TransactionId,
				Status = 0,
				Payout = 0,
				ActualStake = data?.Payload?.BetDataSet[0].ActualStake ?? 0,
				NetStake = data?.Payload?.BetDataSet[0].Stake ?? 0,
				Odds = data?.Payload?.BetDataSet[0].Odds
			}
		};

	public static List<BetResultCheckResponse> BuildBetResultBySettle(SabaPlusEndGameRequest data)
		=> (from item in data?.Payload?.BetDataSet
			select new BetResultCheckResponse
			{
				ErrorCode = 0,
				ErrorMessage = "Success",
				Seq = Guid.NewGuid().ToString(),
				TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
				Payload =
				new BetResultPayloadModel
				{
					TicketNo = item.TicketNo,
					OperatorCustId = data?.Payload?.OperatorCustId,
					GameRoundId = item.GameRoundId,
					TransactionId = item.TransactionId,
					Status = 1,
					ActualStake = item.ActualStake,
					NetStake = item.NetStake,
					Odds = item.Odds,
					Payout = item.Payout,
					GameResult = item.GameResult,
					OtherBetInfo = item.OtherBetInfo,
					SettleDate = item.SettleDate,
				}
			}).ToList();

	public static List<BetResultCheckResponse> BuildBetResultByVoidGame(VoidGameRequest data)
	=> (from item in data?.Payload?.BetDataSet
		select new BetResultCheckResponse
		{
			ErrorCode = 0,
			ErrorMessage = "Success",
			Seq = Guid.NewGuid().ToString(),
			TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
			Payload = new()
			{
				TicketNo = item.TicketNo,
				OperatorCustId = data?.Payload?.OperatorCustId,
				GameRoundId = item.GameRoundId,
				TransactionId = item.TransactionId,
				Status = (short)item.Status,
			}
		}).ToList();

	public static List<BetResultCheckResponse> BuildBetResultByPlaceBetAndSettle(SabaPlusPlaceBetAndSettleRequest data)
	=> (from item in data?.Payload?.BetDataSet
		select new BetResultCheckResponse
		{
			ErrorCode = 0,
			ErrorMessage = "Success",
			Seq = Guid.NewGuid().ToString(),
			TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
			Payload =
			new BetResultPayloadModel
			{
				TicketNo = item?.TicketNo,
				OperatorCustId = data?.Payload?.OperatorCustId,
				GameRoundId = item?.GameRoundId,
				TransactionId = item?.TransactionId,
				Status = 1,
				ActualStake = item?.ActualStake,
				NetStake = item?.NetStake,
				Payout = item?.Payout,
				OtherBetInfo = item?.OtherBetInfo,
				SettleDate = item?.SettleDate,
			}
		}).ToList();

	public static List<BetResultCheckResponse> BuildBetResultByFreeSpin(SabaPlusFreeSpinRequest data)
=> (from item in data?.Payload?.BetDataSet
	select new BetResultCheckResponse
	{
		ErrorCode = 0,
		ErrorMessage = "Success",
		Seq = Guid.NewGuid().ToString(),
		TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
		Payload =
		new BetResultPayloadModel
		{
			TicketNo = item?.TicketNo,
			OperatorCustId = data?.Payload?.OperatorCustId,
			GameRoundId = item?.GameRoundId,
			TransactionId = item?.TransactionId,
			Status = 1,
			ActualStake = item?.ActualStake,
			NetStake = item?.NetStake,
			Payout = item?.Payout,
			OtherBetInfo = item?.OtherBetInfo,
			SettleDate = item?.SettleDate,
		}
	}).ToList();
}