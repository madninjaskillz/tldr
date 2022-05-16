namespace tldr.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; }
    public string PrivateKey { get; set; } = "PK:" + Guid.NewGuid() + Guid.NewGuid() + Guid.NewGuid();
    public string EncryptedPassword { get; set; }

}