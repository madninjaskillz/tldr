using Microsoft.AspNetCore.Components;

namespace tldr.Models
{
    public class Session
    {
        public User LoggedInUser { get; set; } = null;
        public Guid Id { get; set; } = Guid.NewGuid();
        
    }
}
