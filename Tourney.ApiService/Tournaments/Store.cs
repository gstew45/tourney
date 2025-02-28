using System.Collections.Concurrent;

namespace Tourney.ApiService.Tournaments
{
	// Yes this is dumb, just doing for quickness just now, will move to a proper storage and management pattern in due course.
	// Need ability to share tournaments with MatchController for now....
	public static class TournamentsContext
	{
		public static ConcurrentDictionary<Guid, Tournament> Tournaments = new ConcurrentDictionary<Guid, Tournament>();
	}

	public static class MatchesContext
	{
		public static ConcurrentDictionary<Guid, Match> Matches = new ConcurrentDictionary<Guid, Match>();
	}

	public static class BracketContext
	{
		public static ConcurrentDictionary<Guid, IEnumerable<Round>> Brackets = new ConcurrentDictionary<Guid, IEnumerable<Round>>();
	}
}
