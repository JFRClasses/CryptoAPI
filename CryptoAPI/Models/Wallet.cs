namespace CryptoAPI.Models;

public class Wallet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public SystemUser User { get; set; }
    public List<Transaction> Transactions { get; set; }
}