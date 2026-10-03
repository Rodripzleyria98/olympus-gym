using Microsoft.EntityFrameworkCore;
using Olympus.Api.Models;

namespace Olympus.Api.Data;

public static class OlympusDbSeeder
{
    private static readonly Guid AdminId = Guid.Parse("4d49fe51-7b39-4ba0-9937-453cb74c83b4");
    private static readonly Guid TestUserId = Guid.Parse("a9f49e45-aedb-4fda-96c7-8a51a1cf12a7");

    public static async Task InitializeAsync(
        OlympusDbContext dbContext,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Usuarios.AnyAsync(usuario => usuario.Rol == RolUsuario.Admin, cancellationToken))
        {
            var adminPassword = configuration["SeedAdminPassword"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException("Configure SeedAdminPassword before initializing the database.");
            }

            dbContext.Usuarios.Add(new Usuario
            {
                Id = AdminId,
                Nombre = "Olympus",
                Apellido = "Administrador",
                Email = "admin@olympus.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Rol = RolUsuario.Admin,
                FechaRegistro = DateTime.UtcNow
            });
        }

        var testUserPassword = configuration["SeedTestUserPassword"];
        if (!string.IsNullOrWhiteSpace(testUserPassword) &&
            !await dbContext.Usuarios.AnyAsync(usuario => usuario.Email == "prueba@olympus.com", cancellationToken))
        {
            dbContext.Usuarios.Add(new Usuario
            {
                Id = TestUserId,
                Nombre = "Socio",
                Apellido = "Prueba",
                Email = "prueba@olympus.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(testUserPassword),
                Rol = RolUsuario.User,
                FechaRegistro = DateTime.UtcNow
            });
        }

        foreach (var planSeed in MembresiaSeedData.Planes)
        {
            if (!await dbContext.PlanesMembresia.AnyAsync(plan => plan.Id == planSeed.Id, cancellationToken))
            {
                dbContext.PlanesMembresia.Add(new PlanMembresia
                {
                    Id = planSeed.Id,
                    Nombre = planSeed.Nombre,
                    Descripcion = planSeed.Descripcion,
                    Precio = planSeed.Precio,
                    DuracionDias = planSeed.DuracionDias,
                    ClasesIncluidas = planSeed.ClasesIncluidas,
                    IsActivo = planSeed.IsActivo
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}