namespace CryptoAPI.Models;

public class SystemUser
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Wallet> Wallets { get; set; }
}