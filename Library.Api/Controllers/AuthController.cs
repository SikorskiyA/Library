using Library.Api.Auth;
using Library.Api.DTOs;
using Library.Core.Constants;
using Library.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtTokenGenerator jwtTokenGenerator
    )
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Unauthorized(new { Message = "Неверная почта или пароль" });

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
            return Unauthorized(new { Message = "Неверная почта или пароль" });

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count == 0)
            return StatusCode(500, new { Message = "У пользователя не назначена роль" });

        var role = roles[0];
        var jwtToken = _jwtTokenGenerator.GenerateToken(user, role);

        return Ok(
            new LoginResponse
            {
                Token = jwtToken,
                Email = user.Email!,
                Role = role,
            }
        );
    }
}
