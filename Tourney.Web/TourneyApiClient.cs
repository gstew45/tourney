using Tourney.Contract;

namespace Tourney.Web;

public class TourneyApiClient(HttpClient httpClient)
{
	public async Task<TournamentResponse[]> GetTournamentsAsync(int maxItems = 10, CancellationToken cancellationToken = default)
	{
		List<TournamentResponse>? tournaments = null;

		await foreach (var tournament in httpClient.GetFromJsonAsAsyncEnumerable<TournamentResponse>("/tournament", cancellationToken))
		{
			if (tournaments?.Count >= maxItems)
			{
				break;
			}
			if (tournament is not null)
			{
				tournaments ??= [];
				tournaments.Add(tournament);
			}
		}

		return tournaments?.ToArray() ?? [];
	}

	public async Task<TournamentResponse> GetTournamentByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var response = await httpClient.GetFromJsonAsync<TournamentResponse>($"/tournament/{id}", cancellationToken);

		if (response is null)
		{
			throw new Exception("Tournament does not exist");
		}

		return response;
	}

	public async Task<MatchResponse> GetMatchResponseAsync(Guid matchId, CancellationToken cancellationToken = default)
	{
		var response = await httpClient.GetFromJsonAsync<MatchResponse>($"/match/{matchId}", cancellationToken);

		if (response is null)
		{
			throw new Exception("Match does not exist");
		}

		return response;
	}

	public async Task CreateTournamentAsync(CreateTournamentRequest request, CancellationToken cancellationToken = default)
	{
		await httpClient.PostAsJsonAsync("/tournament", request, cancellationToken);
	}

	public async Task StartMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
	{
		var emptyContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
		await httpClient.PostAsync($"/match/{matchId}/start", emptyContent, cancellationToken);
	}

	public async Task EndMatchAsync(Guid matchId, EndMatchRequest request, CancellationToken cancellationToken = default)
	{
		await httpClient.PostAsJsonAsync($"/match/{matchId}/end", request, cancellationToken);
	}

	public async Task<TeamStatsResponse[]> GetLeagueStandingsAsync(Guid tournamentId, CancellationToken cancellationToken = default)
	{
		var response = await httpClient.GetFromJsonAsync<TeamStatsResponse[]>($"/tournament/{tournamentId}/league/standings", cancellationToken);

		if (response is null)
		{
			throw new Exception("Match does not exist");
		}

		return response;
	}

	public async Task<RoundResponse[]> GetBracketPlaceholderAsync(Guid tournamentId, CancellationToken cancellationToken = default)
	{
		var response = await httpClient.GetFromJsonAsync<RoundResponse[]>($"/tournament/{tournamentId}/bracket/placeholder", cancellationToken);

		if (response is null)
		{
			throw new Exception("Match does not exist");
		}

		return response;
	}

	public async Task<RoundResponse[]> GetBracketAsync(Guid tournamentId, CancellationToken cancellationToken = default)
	{
		var response = await httpClient.GetFromJsonAsync<RoundResponse[]>($"/tournament/{tournamentId}/bracket", cancellationToken);

		if (response is null)
		{
			throw new Exception("Match does not exist");
		}

		return response;
	}
}
