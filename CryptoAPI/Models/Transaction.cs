using CryptoAPI.Enums;

namespace CryptoAPI.Models;

public class Transaction
{
    public int Id { get; set; }
    public double Price { get; set; }
    public double CostPerCoin { get; set; }
    public double TotalUSD { get; set; }
    public int WalletId { get; set; }
    public Wallet Wallet { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime Date { get; set; }
}