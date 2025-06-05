using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Api.Services.Attributes;

namespace Api.Services.Handler;

public class RequestServiceHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyDictionary<string, ServiceInfo> _services;

    public RequestServiceHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _services = BuildServiceRegistry();
    }

    private Dictionary<string, ServiceInfo> BuildServiceRegistry()
    {
        var result = new Dictionary<string, ServiceInfo>(StringComparer.OrdinalIgnoreCase);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in assemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (Exception)
            {
                // Skip any assembly that cannot be reflected into
                continue;
            }

            foreach (var type in types)
            {
                var svcAttr = type.GetCustomAttribute<Service>();
                if (svcAttr == null) continue;

                if (string.IsNullOrWhiteSpace(svcAttr.Name))
                    throw new InvalidOperationException(
                        $"Service attribute on {type.FullName} must specify a non-empty Name.");

                if (result.TryGetValue(svcAttr.Name, out var value))
                    throw new InvalidOperationException(
                        $"Duplicate service name '{svcAttr.Name}' found on types '{value.ServiceType.FullName}' and '{type.FullName}'.");

                var methodMap = new Dictionary<string, MethodDetails>(StringComparer.OrdinalIgnoreCase);
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var mAttr = method.GetCustomAttribute<ServiceMethod>();
                    if (mAttr == null) continue;

                    if (string.IsNullOrWhiteSpace(mAttr.Name))
                        throw new InvalidOperationException(
                            $"ServiceMethod attribute on {type.FullName}.{method.Name} must specify a non-empty Name.");

                    if (methodMap.ContainsKey(mAttr.Name))
                        throw new InvalidOperationException(
                            $"Duplicate ServiceMethod name '{mAttr.Name}' in service '{svcAttr.Name}'.");

                    // Ensure attribute's Parameter array (if provided) matches actual signature length
                    var realParams = method.GetParameters();
                    if (mAttr.Parameter != null && mAttr.Parameter.Length != realParams.Length)
                        throw new InvalidOperationException(
                            $"Parameter type count mismatch on ServiceMethod '{mAttr.Name}' in '{type.FullName}'. " +
                            $"Attribute defines {mAttr.Parameter.Length} parameters but method has {realParams.Length}.");

                    // Use the CLR parameter types directly for binding
                    var paramTypes = realParams.Select(p => p.ParameterType).ToArray();

                    methodMap[mAttr.Name] = new MethodDetails
                    {
                        Method = method,
                        ParameterTypes = paramTypes
                    };
                }

                result[svcAttr.Name] = new ServiceInfo
                {
                    ServiceType = type,
                    Methods = methodMap
                };
            }
        }

        return result;
    }

    public async Task<HandlerResult> Handle(RequestDto requestDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestDto.ServiceName))
            return HandlerResult.Failure("ServiceName must be provided.");

        if (!_services.TryGetValue(requestDto.ServiceName, out var serviceInfo))
            return HandlerResult.Failure($"Service '{requestDto.ServiceName}' not found.");

        if (string.IsNullOrWhiteSpace(requestDto.ServiceMethodName))
            return HandlerResult.Failure("ServiceMethodName must be provided.");

        if (!serviceInfo.Methods.TryGetValue(requestDto.ServiceMethodName, out var methodInfo))
            return HandlerResult.Failure(
                $"Method '{requestDto.ServiceMethodName}' not found in service '{requestDto.ServiceName}'.");

        var serviceInstance = _serviceProvider.GetService(serviceInfo.ServiceType);
        if (serviceInstance == null)
            return HandlerResult.Failure(
                $"Service type '{serviceInfo.ServiceType.FullName}' is not registered in the DI container.");

        object[] args;
        try
        {
            args = BindParameters(requestDto.Payload, methodInfo.Method, methodInfo.ParameterTypes, cancellationToken);
        }
        catch (BindingException be)
        {
            return HandlerResult.Failure($"Parameter binding failed: {be.Message}");
        }

        object? invocationResult;
        try
        {
            invocationResult = methodInfo.Method.Invoke(serviceInstance, args);
        }
        catch (TargetInvocationException tie)
        {
            // Unwrap the inner exception so the caller sees the real error
            throw tie.InnerException ?? tie;
        }

        // Handle asynchronous methods
        if (invocationResult is not Task task) return HandlerResult.Success(invocationResult);
        await task.ConfigureAwait(false);

        var returnType = methodInfo.Method.ReturnType;
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
            return HandlerResult.Success();
        var resultProperty = task.GetType().GetProperty("Result");
        var value = resultProperty?.GetValue(task);
        return HandlerResult.Success(value);
    }

    private object[] BindParameters(object? payload, MethodInfo method, Type[] paramTypes,
        CancellationToken cancellationToken)
    {
        var parameters = method.GetParameters();
        if (paramTypes.Length == 0)
            return [];

        JsonElement rootElement;
        if (payload is JsonElement je)
        {
            rootElement = je;
        }
        else if (payload == null)
        {
            // If any non-optional, non-nullable parameter exists, throw
            if ((from p in parameters
                    let t = p.ParameterType
                    where t != typeof(CancellationToken)
                    where !p.HasDefaultValue && (!IsSimpleType(t) && !t.IsClass)
                    select p).Any())
            {
                throw new BindingException("Payload is null but method expects complex parameters.");
            }

            using var doc = JsonDocument.Parse("{}");
            rootElement = doc.RootElement.Clone();
        }
        else
        {
            string jsonString = JsonSerializer.Serialize(payload);
            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                rootElement = doc.RootElement.Clone();
            }
            catch (JsonException jex)
            {
                throw new BindingException($"Invalid JSON payload: {jex.Message}");
            }
        }

        var args = new object[paramTypes.Length];
        var usedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Fast-path: single complex parameter
        if (paramTypes.Length == 1 && !IsSimpleType(paramTypes[0]) &&
            parameters[0].ParameterType != typeof(CancellationToken))
        {
            try
            {
                var complexObj = JsonSerializer.Deserialize(rootElement.GetRawText(), paramTypes[0]);
                if (complexObj != null)
                {
                    args[0] = complexObj;
                    return args;
                }
            }
            catch (JsonException jex)
            {
                throw new BindingException($"Could not bind JSON to type {paramTypes[0].FullName}: {jex.Message}");
            }
        }

        for (int i = 0; i < paramTypes.Length; i++)
        {
            var pInfo = parameters[i];
            var pType = paramTypes[i];
            var pName = pInfo.Name!;
            object? boundValue = null;
            bool isBound = false;

            // 1) CancellationToken injection
            if (pType == typeof(CancellationToken))
            {
                boundValue = cancellationToken;
                isBound = true;
            }
            else
            {
                // 2) Name-based, case-insensitive lookup
                foreach (var prop in rootElement.EnumerateObject()
                             .Where(prop => !usedPropertyNames.Contains(prop.Name)).Where(prop =>
                                 string.Equals(prop.Name, pName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (TryBindElement(prop.Value, pType, out boundValue))
                    {
                        usedPropertyNames.Add(prop.Name);
                        isBound = true;
                    }
                    else if (pInfo.HasDefaultValue)
                    {
                        boundValue = pInfo.DefaultValue;
                        isBound = true;
                    }
                    else
                    {
                        throw new BindingException(
                            $"Parameter '{pName}' of type '{pType.Name}' could not be converted from JSON property '{prop.Name}'.");
                    }

                    break;
                }

                // 3) If not bound and complex, try root → complex
                if (!isBound && !IsSimpleType(pType))
                {
                    try
                    {
                        var obj = JsonSerializer.Deserialize(rootElement.GetRawText(), pType);
                        if (obj != null)
                        {
                            boundValue = obj;
                            isBound = true;
                        }
                    }
                    catch (JsonException)
                    {
                        // Fall through to next strategies
                    }
                }

                // 4) If still not bound and has default value, use it
                if (!isBound && pInfo.HasDefaultValue)
                {
                    boundValue = pInfo.DefaultValue;
                    isBound = true;
                }

                // 5) If still not bound and simple type, attempt to pick an unused JSON property that can convert
                if (!isBound && IsSimpleType(pType))
                {
                    foreach (var prop in rootElement.EnumerateObject())
                    {
                        if (usedPropertyNames.Contains(prop.Name)) continue;
                        if (TryConvertSimpleType(prop.Value, pType, out var tmpSimple))
                        {
                            boundValue = tmpSimple;
                            usedPropertyNames.Add(prop.Name);
                            isBound = true;
                            break;
                        }
                    }
                }

                // 6) If still not bound, assign default (value-type default or null)
                if (!isBound)
                {
                    boundValue = pType.IsValueType ? Activator.CreateInstance(pType)! : null;
                }
            }

            args[i] = boundValue!;
        }

        return args;
    }

    private bool TryBindElement(JsonElement element, Type targetType, out object? value)
    {
        if (IsSimpleType(targetType))
        {
            return TryConvertSimpleType(element, targetType, out value);
        }

        try
        {
            value = JsonSerializer.Deserialize(element.GetRawText(), targetType);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    private bool TryConvertSimpleType(JsonElement element, Type targetType, out object? value)
    {
        value = null;

        // Handle Nullable<T>
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var innerType = Nullable.GetUnderlyingType(targetType)!;
            if (element.ValueKind == JsonValueKind.Null)
            {
                value = null;
                return true;
            }

            if (TryConvertSimpleType(element, innerType, out var innerValue))
            {
                value = innerValue;
                return true;
            }

            return false;
        }

        // Handle Enum
        if (targetType.IsEnum)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString()!;
                if (Enum.TryParse(targetType, str, ignoreCase: true, out var enumVal))
                {
                    value = enumVal;
                    return true;
                }

                return false;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var intVal))
            {
                value = Enum.ToObject(targetType, intVal);
                return true;
            }

            return false;
        }

        try
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                if (!targetType.IsValueType || (Nullable.GetUnderlyingType(targetType) != null))
                {
                    value = null;
                    return true;
                }

                return false;
            }

            if (targetType == typeof(string) && element.ValueKind == JsonValueKind.String)
            {
                value = element.GetString();
                return true;
            }

            if (targetType == typeof(bool) &&
                (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False))
            {
                value = element.GetBoolean();
                return true;
            }

            if (element.ValueKind == JsonValueKind.Number)
            {
                var raw = element.GetRawText();
                value = Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
                return true;
            }

            if (targetType == typeof(Guid) && element.ValueKind == JsonValueKind.String)
            {
                if (Guid.TryParse(element.GetString(), out var g))
                {
                    value = g;
                    return true;
                }

                return false;
            }

            if (targetType == typeof(DateTime) && element.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(element.GetString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out var dt))
                {
                    value = dt;
                    return true;
                }

                return false;
            }

            if (targetType == typeof(DateTimeOffset) && element.ValueKind == JsonValueKind.String)
            {
                if (DateTimeOffset.TryParse(element.GetString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out var dto))
                {
                    value = dto;
                    return true;
                }

                return false;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(Guid)
               || type.IsEnum;
    }

    private class ServiceInfo
    {
        public Type ServiceType { get; init; } = default!;
        public IReadOnlyDictionary<string, MethodDetails> Methods { get; init; } = default!;
    }

    private class MethodDetails
    {
        public MethodInfo Method { get; init; } = default!;
        public Type[] ParameterTypes { get; init; } = Array.Empty<Type>();
    }
}

public class HandlerResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Payload { get; set; }

    public static HandlerResult Success(object? payload = null)
    {
        return new HandlerResult
        {
            Succeeded = true,
            Payload = payload
        };
    }

    public static HandlerResult Failure(string error)
    {
        return new HandlerResult
        {
            Succeeded = false,
            ErrorMessage = error
        };
    }
}

public class BindingException : Exception
{
    public BindingException(string message) : base(message)
    {
    }
}