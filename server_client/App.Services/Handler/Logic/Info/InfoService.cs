using Api.Services.Attributes;
using System.Reflection;

namespace Api.Services.Handler.Logic.Info;

[Service("InfoService", "Provides information about the services")]
public class InfoService
{
    [ServiceMethod("GetInfo", [], "Returns information about the services")]
    public InfoServiceResultDto GetInfo()
    {
        var services = new List<ServiceInfo>();

        var assembly = Assembly.GetExecutingAssembly();
        var serviceTypes = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<Service>() != null);

        foreach (var serviceType in serviceTypes)
        {
            var serviceAttribute = serviceType.GetCustomAttribute<Service>();
            if (serviceAttribute == null)
                continue;

            var serviceMethods = serviceType.GetMethods()
                .Where(method => method.GetCustomAttribute<ServiceMethod>() != null)
                .Select(method =>
                {
                    var methodAttribute = method.GetCustomAttribute<ServiceMethod>();
                    var parameters = methodAttribute?.Parameter?.Select(p => p.Name).ToArray() ?? [];

                    return new ServiceMethodInfo(
                        methodAttribute?.Name ?? method.Name,
                        parameters,
                        methodAttribute?.Description ?? ""
                    );
                })
                .ToArray();

            services.Add(new ServiceInfo(
                serviceAttribute.Name,
                serviceType.Name,
                serviceMethods,
                serviceAttribute.Description
            ));
        }

        return new InfoServiceResultDto(services.ToArray());
    }
}