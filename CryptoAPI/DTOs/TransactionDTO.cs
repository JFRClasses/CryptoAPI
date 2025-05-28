using CryptoAPI.Enums;

namespace CryptoAPI.DTOs;

public class TransactionDTO
{
    public int Id { get; set; }
    public double Price { get; set; }
    public double CostPerCoin { get; set; }
    public int WalletId { get; set; }
    public TransactionType TransactionType { get; set; }
    public double TotalUSD { get; set; }
    public DateTime Date { get; set; }
}