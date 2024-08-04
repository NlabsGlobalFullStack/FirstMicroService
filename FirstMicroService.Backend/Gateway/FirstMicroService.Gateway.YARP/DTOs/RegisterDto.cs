namespace FirstMicroService.Gateway.YARP.DTOs;

public sealed record RegisterDto(
    string UserName,
    string Password);
