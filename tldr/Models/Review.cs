namespace tldr.Models;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public string Tldr { get; set; }

    public List<ReviewVote> Votes { get; set; } = new List<ReviewVote>();
    public string Username { get; set; }
}

public class DynamicReview : Review
{
    public int Stars { get; set; }
}