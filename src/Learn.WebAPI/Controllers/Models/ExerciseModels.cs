namespace Learn.WebAPI.Controllers.Models;

public record VoteRequest
{
    public bool IsUpvote { get; init; }
}
