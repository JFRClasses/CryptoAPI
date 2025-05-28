namespace CryptoAPI;
using BC = BCrypt.Net.BCrypt;
public static class Tools
{
    public static string HashPassword(string password) => BC.HashPassword(password,10);
    public static bool DecodePassword(string loginPassword,string hashPassword) => BC.Verify(loginPassword,hashPassword);

}