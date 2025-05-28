namespace CryptoAPI.DTOs;

public class WalletDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double Balance { get; set; }
    public double USDBalance { get; set; }
}