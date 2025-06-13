using CryptoAPI.Enums;

namespace CryptoAPI.DTOs;

public class TransactionDTO
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public double CostPerCoin { get; set; }
    public double TotalUSD { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime Date { get; set; }
}