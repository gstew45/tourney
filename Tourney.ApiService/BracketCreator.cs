using Tourney.ApiService.Tournaments;
using Tourney.Contract;

namespace Tourney.ApiService
{
	// Tournament
	// Tournaments consist of n number of teams and two stages, a group (league) stage to determine seeding, followed by a bracket knockout to determine champion
	// Each match in a league and bracket, consists of a best of 3 legs match,
	// Each league game, a contestant gets points based on leg wins + an extra point based on match win.
	// At end of group stage, contestants are ranked from 1 to n based on their league position and bracket starts
	// Bracket matches are seeded as per these rules 1st vs last, 2nd vs 2nd last and so on.
	// Brackets matches are again a best of 3 legs, winner progresses to next match until champion

	public static class TournamentManager
	{
		public static Tournament CreateTournament(string name, IEnumerable<string> players, int leagueGameCount)
		{
			// players count has to be a power of 2 to create an even bracket (what we want for now)
			int playersCount = players.Count();
			if (!IsPowerOfTwo(playersCount))
			{
				throw new ArgumentException("Players count must be a count that is a power of 2 (e.g., 1, 2, 4, 8, 16, etc.)");
			}

			// Based on amount of league games, generate all combinations of head to head play.
			List<List<Match>> leagueMatches = GenerateMatchSchedule(players.ToList(), leagueGameCount);
			leagueMatches.ForEach(round => round.ForEach(match => MatchesContext.Matches.TryAdd(match.Id, match)));

			return new Tournament()
			{
				Name = name,
				Players = players.ToList(),
				LeagueMatches = leagueMatches.SelectMany(x => x).ToList()
			};
		}

		public static List<Round> GenerateBracket(List<Match> leagueMatches)
		{
			List<TeamStats> leagueTable = GetLeagueStandings(leagueMatches);

			// Create a list to hold the knockout bracket matches
			List<Round> bracket = new List<Round>();

			// First round: Generate matches based on the league standings
			int totalTeams = leagueTable.Count;
			Round firstRound = new Round() { Number = 1 };
			bracket.Add(firstRound);

			for (int i = 0; i < totalTeams / 2; i++)
			{
				var homePlayer = leagueTable[i].Name; // 1st place
				var awayPlayer = leagueTable[totalTeams - 1 - i].Name; // last place

				firstRound.Matches.Add(new Match
				{
					HomePlayer = homePlayer,
					AwayPlayer = awayPlayer,
				});
			}

			// Generate matches for subsequent rounds (empty matches as placeholders)
			int round = 2;
			int remainingTeams = totalTeams / 2;
			while (remainingTeams > 1)
			{
				Round subsequentRound = new Round() { Number = round };
				bracket.Add(subsequentRound);

				for (int i = 0; i < remainingTeams / 2; i++)
				{
					subsequentRound.Matches.Add(new Match
					{
						HomePlayer = null, // Placeholder, to be filled in later
						AwayPlayer = null, // Placeholder, to be filled in later
					});
				}
				round++;
				remainingTeams /= 2; // Half the teams progress to the next round
			}

			return bracket;
		}

		public static List<TeamStats> GetLeagueStandings(List<Match> matches)
		{
			Dictionary<string, TeamStats> teamStats = new Dictionary<string, TeamStats>();

			foreach (Match match in matches)
			{
				if (!teamStats.TryGetValue(match.HomePlayer, out TeamStats? homeTeamStats))
				{
					homeTeamStats = new TeamStats()
					{
						Name = match.HomePlayer
					};

					teamStats.Add(match.HomePlayer, homeTeamStats);
				}

				if (!teamStats.TryGetValue(match.AwayPlayer, out TeamStats? awayTeamStats))
				{
					awayTeamStats = new TeamStats()
					{
						Name = match.AwayPlayer
					};

					teamStats.Add(match.AwayPlayer, awayTeamStats);
				}

				if (match.HomeScore < 0 && match.AwayScore < 0)
				{
					// Match hasn't been played, continue to next result.
					continue;
				}

				if (match.HomeScore != 2 && match.AwayScore != 2)
				{
					// Match has not been completed, a player has to get to 2 to win.
					continue;
				}

				homeTeamStats.Played++;
				homeTeamStats.LegWins += match.HomeScore;

				awayTeamStats.Played++;
				awayTeamStats.LegWins += match.AwayScore;

				if (match.HomeScore > match.AwayScore)
				{
					// Home win
					homeTeamStats.MatchWins++;
				}
				else
				{
					// Away win
					awayTeamStats.MatchWins++;
				}
			}

			return teamStats.Values
				.OrderByDescending(x => x.TotalPoints)
				.ThenByDescending(x => x.MatchWins)
				.ThenByDescending(x => x.LegWins)
				.ToList();
		}

		private static List<List<Match>> GenerateMatchSchedule(List<string> players, int n)
		{
			List<Match> matches = new List<Match>();

			// Generate all unique matchups (Alice vs Bob but not Bob vs Alice separately)
			for (int count = 0; count < n; count++) // Repeat n times
			{
				for (int i = 0; i < players.Count - 1; i++)
				{
					for (int j = i + 1; j < players.Count; j++)
					{
						Match match = new()
						{
							HomePlayer = players[i],
							AwayPlayer = players[j]
						};
						matches.Add(match);
					}
				}
			}

			List<List<Match>> rounds = new List<List<Match>>();

			// Distribute matches into rounds fairly
			while (matches.Count > 0)
			{
				HashSet<string> activePlayers = new HashSet<string>();
				List<Match> round = new List<Match>();

				for (int i = 0; i < matches.Count; i++)
				{
					var match = matches[i];

					// If neither player is already in this round, schedule the match
					if (!activePlayers.Contains(match.HomePlayer) && !activePlayers.Contains(match.AwayPlayer))
					{
						round.Add(match);
						activePlayers.Add(match.HomePlayer);
						activePlayers.Add(match.AwayPlayer);
					}
				}

				// Remove scheduled matches from the match list
				foreach (var match in round)
				{
					matches.Remove(match);
				}

				rounds.Add(round);
			}

			return rounds;
		}

		private static bool IsPowerOfTwo(int n)
		{
			return (n > 0) && (n & (n - 1)) == 0;
		}
	}

	public class Round
	{
		public int Number { get; set; }
		public List<Match> Matches { get; set; } = new List<Match>();
	}

	public class Match
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public MatchStatus Status { get; set; } = MatchStatus.NotStarted;
		public string HomePlayer { get; set; }
		public string AwayPlayer { get; set; }
		public int HomeScore { get; set; } = 0;
		public int AwayScore { get; set; } = 0;
	}

	public class Tournament
	{
		public Guid Id { get; } = Guid.NewGuid();
		public TournamentStatus Status { get; set; } = TournamentStatus.NotStarted;
		public string Name { get; set; }
		public List<string> Players { get; set; }
		public List<Match> LeagueMatches { get; set; } = new List<Match>();
		public List<Match> KnockoutBracket { get; set; } = new List<Match>();
	}

	public class TeamStats
	{
		public string Name { get; set; }

		public int Played { get; set; }
		public int TotalPoints => MatchWins + LegWins;
		public int MatchWins { get; set; }
		public int LegWins { get; set; }
	}
}
