using System.Text.Json;
using CryptoAPI;
using CryptoAPI.DTOs;
using CryptoAPI.Enums;
using CryptoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapPost("/register", async (UserCDTO userDto, ApplicationDbContext db) =>
{
    var existingUser = await db.SystemUser.FirstOrDefaultAsync(u => u.Email == userDto.Email);
    if (existingUser != null)
    {
        return Results.Ok(new BodyResponse<LoginResponse>
        {
            Body = new LoginResponse
            {
                isLogged = false,
                Message = "El usuario ya existe.",
                UserId = 0
            },
            MessageTitle = "Error",
            MessageContent = "El usuario ya está registrado."
        });
    }

    var user = new SystemUser
    {
        Email = userDto.Email,
        Password = Tools.HashPassword(userDto.Password)
    };

    try
    {
        db.SystemUser.Add(user);
        await db.SaveChangesAsync();

        var wallet = new Wallet
        {
            UserId = user.Id
        };
        db.Wallet.Add(wallet);
        await db.SaveChangesAsync();

        return Results.Ok(new BodyResponse<LoginResponse>
        {
            Body = new LoginResponse
            {
                isLogged = true,
                Message = "Registro exitoso",
                UserId = user.Id
            },
            MessageTitle = "Éxito",
            MessageContent = "Usuario y wallet creados correctamente."
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new BodyResponse<LoginResponse>
        {
            Body = new LoginResponse
            {
                isLogged = false,
                Message = "Error al registrar",
                UserId = 0
            },
            MessageTitle = "Error",
            MessageContent = $"No se pudo crear el usuario o la wallet: {ex.Message}"
        });
    }
});

app.MapPost("/login", async (UserCDTO userDto, ApplicationDbContext db) =>
{
    var user = await db.SystemUser.FirstOrDefaultAsync(u => u.Email == userDto.Email);
    if (user == null)
    {
        return Results.Ok(new BodyResponse<LoginResponse>
        {
            Body = new LoginResponse
            {
                isLogged = false,
                Message = "Correo o contraseña incorrectos.",
                UserId = 0
            },
            MessageTitle = "Error",
            MessageContent = "No se encontró el usuario."
        });
    }

    var result = Tools.DecodePassword(userDto.Password, user.Password);
    if (result)
    {
        return Results.Ok(new BodyResponse<LoginResponse>
        {
            Body = new LoginResponse
            {
                isLogged = true,
                Message = "Inicio de sesión exitoso.",
                UserId = user.Id
            },
            MessageTitle = "Éxito",
            MessageContent = "Usuario autenticado correctamente."
        });
    }

    return Results.Ok(new BodyResponse<LoginResponse>
    {
        Body = new LoginResponse
        {
            isLogged = false,
            Message = "Correo o contraseña incorrectos.",
            UserId = 0
        },
        MessageTitle = "Error",
        MessageContent = "Contraseña incorrecta."
    });
});


app.MapGet("/bitcoin/price", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var response = await client.GetAsync("https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd");

    if (!response.IsSuccessStatusCode)
    {
        return Results.Ok(new BodyResponse<object>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = "No se pudo obtener el precio del Bitcoin."
        });
    }

    var json = await response.Content.ReadAsStringAsync();

    return Results.Ok(new BodyResponse<object>
    {
        Body = JsonSerializer.Deserialize<object>(json),
        MessageTitle = "Éxito",
        MessageContent = "Precio actual del Bitcoin obtenido correctamente."
    });
});


app.MapGet("/user/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var user = await db.SystemUser
        .Include(u => u.Wallets)
        .ThenInclude(w => w.Transactions)
        .FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
    {
        return Results.Ok(new BodyResponse<SystemUser>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = "Usuario no encontrado."
        });
    }

    return Results.Ok(new BodyResponse<SystemUser>
    {
        Body = user,
        MessageTitle = "Éxito",
        MessageContent = "Usuario encontrado correctamente."
    });
});


app.MapPost("/wallet/transaction", async (TransactionCDTO dto, ApplicationDbContext db) =>
{
    if (dto.TransactionType != TransactionType.BUY && dto.TransactionType != TransactionType.SELL)
    {
        return Results.Ok(new BodyResponse<TransactionDTO>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = "Solo se permiten transacciones de tipo Buy o Sell."
        });
    }

    var wallet = await db.Wallet
                         .Include(w => w.Transactions)
                         .FirstOrDefaultAsync(w => w.Id == dto.WalletId);

    if (wallet == null)
    {
        return Results.Ok(new BodyResponse<TransactionDTO>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = "Wallet no encontrada."
        });
    }

    double totalBought = wallet.Transactions
        .Where(t => t.TransactionType == TransactionType.BUY)
        .Sum(t => t.Amount);

    double totalSold = wallet.Transactions
        .Where(t => t.TransactionType == TransactionType.SELL)
        .Sum(t => t.Amount);

    double balanceBTC = totalBought - totalSold;

    if (dto.TransactionType == TransactionType.SELL && dto.Amount > balanceBTC)
    {
        return Results.Ok(new BodyResponse<TransactionDTO>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = $"No puedes vender {dto.Amount} BTC. Solo tienes {balanceBTC} BTC disponibles."
        });
    }

    var transaction = new Transaction
    {
        WalletId = wallet.Id,
        Amount = dto.Amount,
        CostPerCoin = dto.CostPerCoin,
        TotalUSD = dto.Amount * dto.CostPerCoin,
        TransactionType = dto.TransactionType,
        Date = DateTime.UtcNow
    };

    db.Transaction.Add(transaction);
    await db.SaveChangesAsync();

    var result = new TransactionDTO
    {
        Id = transaction.Id,
        Amount = transaction.Amount,
        CostPerCoin = transaction.CostPerCoin,
        TotalUSD = transaction.TotalUSD,
        TransactionType = transaction.TransactionType,
        Date = transaction.Date
    };

    return Results.Ok(new BodyResponse<TransactionDTO>
    {
        Body = result,
        MessageTitle = "Éxito",
        MessageContent = "Transacción registrada correctamente."
    });
});
app.MapGet("/wallet/{walletId:int}/transactions/latest", async (int walletId, ApplicationDbContext db) =>
{
    var exists = await db.Wallet.AnyAsync(w => w.Id == walletId);

    if (!exists)
    {
        return Results.Ok(new BodyResponse<List<TransactionDTO>>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = "Wallet no encontrada."
        });
    }

    var latestTransactions = await db.Transaction
        .Where(t => t.WalletId == walletId)
        .OrderByDescending(t => t.Date)
        .Take(10)
        .Select(t => new TransactionDTO
        {
            Id = t.Id,
            Amount = t.Amount,
            CostPerCoin = t.CostPerCoin,
            TotalUSD = t.TotalUSD,
            TransactionType = t.TransactionType,
            Date = t.Date
        })
        .ToListAsync();

    return Results.Ok(new BodyResponse<List<TransactionDTO>>
    {
        Body = latestTransactions,
        MessageTitle = "Éxito",
        MessageContent = "Últimas 10 transacciones obtenidas correctamente."
    });
});

app.MapGet("/wallet/{walletId:int}/transactions", async (int walletId, ApplicationDbContext db) =>
{
    var wallet = await db.Wallet
        .Include(w => w.Transactions)
        .FirstOrDefaultAsync(w => w.Id == walletId);

    if (wallet == null)
    {
        return Results.Ok(new BodyResponse<List<Transaction>>
        {
            Body = null,
            MessageTitle = "Error",
            MessageContent = "Wallet no encontrada."
        });
    }

    return Results.Ok(new BodyResponse<List<Transaction>>
    {
        Body = wallet.Transactions,
        MessageTitle = "Éxito",
        MessageContent = "Transacciones obtenidas correctamente."
    });
});
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
