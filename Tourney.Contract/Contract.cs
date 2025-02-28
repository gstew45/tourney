namespace Tourney.Contract
{
	public enum TournamentStatus
	{
		NotStarted,
		InProgress,
		Complete
	};

	public record TournamentResponse(
		Guid Id,
		string Name,
		TournamentStatus Status,
		IEnumerable<string> Players,
		IEnumerable<MatchResponse> LeagueMatches,
		IEnumerable<MatchResponse> BracketMatches);
	
	public record MatchResponse(
		Guid Id,
		MatchStatus Status,
		string HomePlayer,
		string AwayPlayer,
		int HomeScore,
		int AwayScore);

	public enum MatchStatus
	{
		NotStarted,
		InProgress,
		Complete
	}

	public enum MatchType
	{
		League,
		Bracket
	};

	public record CreateTournamentRequest(string Name, IEnumerable<string> Players, int LeagueRounds);

	public record TeamStatsResponse(string Name,
		int Played,
		int TotalPoints,
		int MatchWins,
		int LegWins);

	public record RoundResponse(int Number,
		IEnumerable<MatchResponse> Matches);


	public record LegScoreStat(double Average, int Total, int? Checkout);
	public record EndMatchRequest(
		string HomePlayer,
		string AwayPlayer,
		int HomeLegsWon,
		int AwayLegsWon,
		IEnumerable<LegScoreStat> HomeLegScores,
		IEnumerable<LegScoreStat> AwayLegScores);
}
