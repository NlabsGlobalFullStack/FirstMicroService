namespace FirstMicroService.Gateway.YARP.DTOs;

public sealed record LoginDto(
    string UserName,
    string Password);
