namespace Api.Services.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class Service(string name, string description = "") : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
}