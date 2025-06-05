namespace Api.Services.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ServiceMethod(string name, Type[] parameter, string description = "")
    : Attribute
{
    public string Name { get; } = name;
    public Type[] Parameter { get; } = parameter;
    public string Description { get; } = description;
}