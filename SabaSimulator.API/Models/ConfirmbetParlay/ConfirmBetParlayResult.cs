namespace SabaSimulator.API.Models.ConfirmbetParlay;

public record ConfirmBetParlayResult(
	string status,
	decimal balance,
	string? msg
);