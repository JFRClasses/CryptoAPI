namespace CryptoAPI.DTOs;

public class BodyResponse<T>
{
    public T Body { get; set; }
    public string MessageTitle { get; set; }
    public string MessageContent { get; set; }
}