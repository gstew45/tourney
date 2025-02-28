using Microsoft.AspNetCore.Mvc;
using Tourney.Contract;

namespace Tourney.ApiService.Tournaments
{

	[Route("[controller]")]
	[ApiController]
	public class TournamentController : ControllerBase
	{
		[HttpGet(Name = "GetTournaments")]
		public ActionResult<IEnumerable<TournamentResponse>> GetTournaments()
		{
			var responses = TournamentsContext.Tournaments.Values
				.Select(t =>
					new TournamentResponse(
						t.Id,
						t.Name,
						t.Status,
						t.Players,
						t.LeagueMatches.Select(x => new MatchResponse(x.Id, x.Status, x.HomePlayer, x.AwayPlayer, x.HomeScore, x.AwayScore)),
						t.KnockoutBracket.Select(x => new MatchResponse(x.Id, x.Status, x.HomePlayer, x.AwayPlayer, x.HomeScore, x.AwayScore))));

			return Ok(responses);
		}

		[HttpGet("{id}", Name = "GetTournamentById")]
		public ActionResult<TournamentResponse> GetTournament(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out Tournament? tournament))
			{
				throw new Exception($"Tournament with {id} does not exist");
			}

			var response = new TournamentResponse(
						tournament.Id,
						tournament.Name,
						tournament.Status,
						tournament.Players,
						tournament.LeagueMatches.Select(x => new MatchResponse(x.Id, x.Status, x.HomePlayer, x.AwayPlayer, x.HomeScore, x.AwayScore)),
						tournament.KnockoutBracket.Select(x => new MatchResponse(x.Id, x.Status, x.HomePlayer, x.AwayPlayer, x.HomeScore, x.AwayScore)));

			return Ok(response);
		}

		[HttpPost(Name = "CreateTournament")]
		public ActionResult<Tournament> CreateTournament(CreateTournamentRequest request)
		{
			Tournament tournament = TournamentManager.CreateTournament(request.Name, request.Players, request.LeagueRounds);
			TournamentsContext.Tournaments.TryAdd(tournament.Id, tournament);

			return Ok(tournament);
		}

		[HttpPut("{id}/start", Name = "StartTournament")]
		public ActionResult StartTournament(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out Tournament? tournament))
			{
				throw new Exception($"Tournament with {id} does not exist");
			}

			tournament.Status = TournamentStatus.InProgress;

			return NoContent();
		}

		[HttpPut("{id}/ends", Name = "EndTournament")]
		public ActionResult EndTournament(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out Tournament? tournament))
			{
				throw new Exception($"Tournament with {id} does not exist");
			}

			tournament.Status = TournamentStatus.Complete;

			return NoContent();
		}

		[HttpGet("{id}/league")]
		public ActionResult<IEnumerable<Match>> GetLeageuMatches(Guid tournamentId)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(tournamentId, out var tournament))
			{
				throw new Exception($"Tournament with id {tournamentId} does not exist");
			}

			return Ok(tournament.LeagueMatches);
		}

		[HttpGet("{id}/league/standings")]
		public ActionResult<IEnumerable<TeamStats>> GetLeagueStandings(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out Tournament? tournament))
			{
				throw new Exception($"Tournament with {id} does not exist");
			}

			List<TeamStats> leagueTable = TournamentManager.GetLeagueStandings(tournament.LeagueMatches);
			return Ok(leagueTable);
		}

		[HttpGet("{id}/league/next")]
		public ActionResult<Match> GetNextLeagueMatch(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out var tournament))
			{
				throw new Exception($"Tournament with id {id} does not exist");
			}

			var nextMatch = tournament.LeagueMatches.FirstOrDefault(x => x.Status == MatchStatus.NotStarted);
			return Ok(nextMatch);
		}

		[HttpGet("{id}/bracket/placeholder")]
		public ActionResult<IEnumerable<Round>?> GetBracketPlaceholderAsync(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out var tournament))
			{
				throw new Exception($"Tournament with id {id} does not exist");
			}

			var bracket = TournamentManager.GenerateBracket(tournament.LeagueMatches);

			return Ok(bracket);
		}

		[HttpGet("{id}/bracket")]
		public ActionResult<IEnumerable<Round>?> GetBracketAsync(Guid id)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(id, out var tournament))
			{
				throw new Exception($"Tournament with id {id} does not exist");
			}

			if (BracketContext.Brackets.TryGetValue(id, out var bracket))
			{
				Round firstRound = bracket.First();
				if (firstRound.Matches.All(x => x.Status != MatchStatus.Complete))
				{
					return Ok(bracket);
				}

				Match matchOne = firstRound.Matches[0];
				Match matchTwo = firstRound.Matches[1];

				Round secondRound = bracket.ElementAt(1);
				Match final = secondRound.Matches.First();
				final.HomePlayer = GetWinner(matchOne);
				final.AwayPlayer = GetWinner(matchTwo);
				return Ok(bracket);
			}

			bracket = TournamentManager.GenerateBracket(tournament.LeagueMatches);

			foreach (var round in bracket)
			{
				foreach (var match in round.Matches)
				{
					MatchesContext.Matches.TryAdd(match.Id, match);
					tournament.KnockoutBracket.Add(match);
				}
			}

			BracketContext.Brackets.TryAdd(id, bracket);

			return Ok(bracket);

			string? GetWinner(Match match)
			{
				if (match.Status == MatchStatus.Complete)
				{
					return match.HomeScore > match.AwayScore ? match.HomePlayer : match.AwayPlayer;
				}

				return string.Empty;
			}
		}

		[HttpGet("{id}/bracket/next")]
		public ActionResult<Match> GetNextBracketMatch(Guid tournamentId)
		{
			if (!TournamentsContext.Tournaments.TryGetValue(tournamentId, out var tournament))
			{
				throw new Exception($"Tournament with id {tournamentId} does not exist");
			}

			var nextMatch = tournament.LeagueMatches.FirstOrDefault(x => x.Status == MatchStatus.NotStarted);
			return Ok(nextMatch);
		}
	}
}
