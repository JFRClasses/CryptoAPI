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

app.Run();
