namespace Api.Services.Handler.Logic.Info;

public record InfoServiceResultDto(ServiceInfo[] Services);
public record ServiceInfo(string Name, string Route, ServiceMethodInfo[] Methods, string Description = "");
public record ServiceMethodInfo(string Name, string[] Parameters, string Description = "");
