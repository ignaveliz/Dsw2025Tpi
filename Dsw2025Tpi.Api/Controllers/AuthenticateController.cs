using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticateController> _logger;

    public AuthenticateController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        JwtTokenService jwtTokenService,
        ILogger<AuthenticateController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        _logger.LogInformation("Intento de login para el usuario: {Username}", request.Username);
        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado: {Username}", request.Username);
            return Unauthorized("Usuario o contraseña incorrectos");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Contraseña incorrecta para el usuario: {Username}", request.Username);
            return Unauthorized("Usuario o contraseña incorrectos");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        if (role == null) 
        { 
            _logger.LogWarning("El usuario no tiene roles asignados: {Username}", request.Username);
            return Unauthorized("El usuario no tiene roles registrados en el sistema"); 
        }

        var token = _jwtTokenService.GenerateToken(request.Username,role);
        _logger.LogInformation("Login exitoso para el usuario: {Username}", request.Username);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        _logger.LogInformation("Intento de registro para el usuario: {Username}", model.Username);
        var user = new IdentityUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        _logger.LogInformation("Resultado del registro para el usuario: {Username} - Succeeded: {Succeeded}", model.Username, result.Succeeded);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Error en el registro para el usuario: {Username} - Errors: {Errors}", model.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }


        var role = model.Role ?? "Usuario";
        await _userManager.AddToRoleAsync(user, role);

        // Opcional: enviar email de confirmación, etc.
        _logger.LogInformation("Usuario registrado correctamente: {Username} con rol {Role}", model.Username, role);
        return Ok("Usuario registrado correctamente.");
    }
}
