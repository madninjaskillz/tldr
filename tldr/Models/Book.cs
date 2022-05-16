namespace tldr.Models
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ASIN { get; set; }
        public List<Review> Review { get; set; } = new List<Review>();
    }
}
