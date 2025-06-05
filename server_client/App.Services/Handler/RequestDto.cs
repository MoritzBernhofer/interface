namespace Api.Services.Handler;

public record RequestDto(string ServiceName, string ServiceMethodName, object Payload);
