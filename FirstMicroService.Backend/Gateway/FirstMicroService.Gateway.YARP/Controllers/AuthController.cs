using FirstMicroService.Gateway.YARP.Context;
using FirstMicroService.Gateway.YARP.DTOs;
using FirstMicroService.Gateway.YARP.Models;
using FirstMicroService.Gateway.YARP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nlabs.Result;

namespace FirstMicroService.Gateway.YARP.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto request, CancellationToken cancellationToken = default)
    {
        bool isUserNameExists = await context.Users.AnyAsync(p => p.UserName == request.UserName, cancellationToken);

        if (isUserNameExists)
        {
            var errorMessages = Result<string>.Failure("Username is already taken!");
            return BadRequest(errorMessages);
        }

        User user = new()
        {
            UserName = request.UserName,
            Password = request.Password
        };

        await context.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var successMessages = Result<string>.Succeed("User registration successfull");

        return Ok(successMessages);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto request, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(p => p.UserName == request.UserName, cancellationToken);

        if (user is null)
        {
            var errorMessages = Result<string>.Failure("User notfound!");
            return BadRequest(errorMessages);
        }

        var jwtProvider = new JwtProvider(configuration);

        string token = jwtProvider.CreateToken(user);

        var successMessages = Result<string>.Succeed(token);
        return Ok(successMessages);
    }
}
