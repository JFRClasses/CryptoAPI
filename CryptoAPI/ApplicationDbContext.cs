using CryptoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoAPI;

public class ApplicationDbContext : DbContext
{
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallet { get; set; }
    public DbSet<SystemUser> SystemUser { get; set; }
    public DbSet<Transaction> Transaction { get; set; }
}