namespace CryptoAPI.DTOs;

public class LoginResponse
{
    public string Message { get; set; }
    public int UserId { get; set; }
    public bool isLogged { get; set; }
}