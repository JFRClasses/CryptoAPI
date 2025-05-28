using CryptoAPI;
using CryptoAPI.DTOs;
using CryptoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

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
        return Results.Ok(new BodyResponse<bool>
        {
            Body = false,
            MessageTitle = "Error",
            MessageContent = "El usuario ya existe."
        });
    }

    var user = new SystemUser
    {
        Email = userDto.Email
    };

    user.Password = Tools.HashPassword(userDto.Password);

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

        return Results.Ok(new BodyResponse<bool>
        {
            Body = true,
            MessageTitle = "Éxito",
            MessageContent = "Usuario y wallet creados correctamente."
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new BodyResponse<bool>
        {
            Body = false,
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
        return Results.Ok(new LoginResponse
        {
            isLogged = false,
            Message = "Correo o contraseña incorrectos."
        });
    }

    var result = Tools.DecodePassword(user.Password, userDto.Password);
    if (result)
    {
        return Results.Ok(new LoginResponse
        {
            isLogged = true,
            Message = "Inicio de sesión exitoso."
        });
    }

    return Results.Ok(new LoginResponse
    {
        isLogged = false,
        Message = "Correo o contraseña incorrectos."
    });
});

app.Run();
