using Microsoft.AspNetCore.Mvc;
using Tourney.Contract;

namespace Tourney.ApiService.Tournaments
{
	[Route("[controller]")]
	[ApiController]
	public class MatchController : ControllerBase
	{
		[HttpGet("{matchId}")]
		public ActionResult<MatchResponse> GetById(Guid matchId)
		{
			if (!MatchesContext.Matches.TryGetValue(matchId, out var match))
			{
				throw new Exception($"Match with id {matchId} does not exist");
			}

			var response = new MatchResponse(
				match.Id,
				match.Status,
				match.HomePlayer,
				match.AwayPlayer,
				match.HomeScore,
				match.AwayScore);

			return Ok(response);
		}

		[HttpPost("{matchId}/start")]
		public ActionResult StartMatch(Guid matchId)
		{
			if (!MatchesContext.Matches.TryGetValue(matchId, out var match))
			{
				throw new Exception($"Match with id {matchId} does not exist");
			}

			if (match != null)
			{
				match.Status = MatchStatus.InProgress;
			}

			return NoContent();
		}

		[HttpPost("{matchId}/end")]
		public ActionResult EndMatch(Guid matchId, EndMatchRequest request)
		{
			if (!MatchesContext.Matches.TryGetValue(matchId, out var match))
			{
				throw new Exception($"Match with id {matchId} does not exist");
			}

			if (match != null)
			{
				match.HomeScore = request.HomeLegsWon;
				match.AwayScore= request.AwayLegsWon;
				match.Status = MatchStatus.Complete;
			}

			return NoContent();
		}
	}

}
