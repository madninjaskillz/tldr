namespace tldr.Models;

public class ReviewVote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ReviewId { get; set; }
    public Vote Vote { get; set; } 

}