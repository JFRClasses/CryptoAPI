using CryptoAPI.Enums;

namespace CryptoAPI.DTOs;

public class TransactionCDTO
{
    public int WalletId { get; set; }
    public double Amount { get; set; }          
    public double CostPerCoin { get; set; }     
    public TransactionType TransactionType { get; set; } 
}