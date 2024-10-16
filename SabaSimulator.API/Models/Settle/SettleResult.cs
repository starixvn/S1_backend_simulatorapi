namespace SabaSimulator.API.Models.Settle;

public record SettleResult
(
	string status,
	string? msg = null
);