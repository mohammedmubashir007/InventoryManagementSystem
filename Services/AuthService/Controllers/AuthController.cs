using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Helpers;
using AuthService.Data;
using AuthService.Interfaces;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;

    public AuthController(AuthDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if(await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("User already exists.");
        }

        var hashedPassword = PasswordHasher.Hash(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User Registered successfully!");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = _tokenService.GetToken(user.Username, user.Email);


        var response = new AuthResponse
        {
            Username = user.Username,
            Email = user.Email,
            Token = token
        };

        return Ok(response);

    }


    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var username = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

        return Ok(new
        {
            Username = username,
            Email = email,
            Message = "You have accessed a protected endpoint!"
        });
    }



    //private string GenerateJwtToken(User user)
    //{
    //    var jwtSettings = _config.GetSection("JwtSettings");
    //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
    //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //    var claims = new[]
    //    {
    //        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
    //        new Claim("username", user.Username),
    //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    //    };

    //    var token = new JwtSecurityToken(
    //        issuer: jwtSettings["Issuer"],
    //        audience: jwtSettings["Audience"],
    //        claims: claims,
    //        expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
    //        signingCredentials: creds
    //    );

    //    return new JwtSecurityTokenHandler().WriteToken(token);
    //}
}