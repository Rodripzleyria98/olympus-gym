using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Olympus.Api.Data;
using Olympus.Api.Dtos;
using Olympus.Api.Models;

namespace Olympus.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(OlympusDbContext dbContext, IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UsuarioDto>> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken))
        {
            return Conflict(new { message = "El correo ya está registrado." });
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = RolUsuario.User,
            FechaRegistro = DateTime.UtcNow
        };

        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new UsuarioDto(
            usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Rol.ToString()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { message = "Correo o contraseña incorrectos." });
        }

        var expiration = DateTime.UtcNow.AddDays(configuration.GetValue("Jwt:ExpireDays", 7));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return Ok(new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiration,
            new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Rol.ToString())));
    }
}