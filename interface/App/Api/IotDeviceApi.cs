using App.Database;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

public static class IotDeviceApi
{
    public static void MapIotDeviceApi(this WebApplication app)
    {
        var group = app.MapGroup("Devices");

        group.MapGet("/", GetAllDevices);
        group.MapPost("/", AddDevice);
        group.MapDelete("/", DeleteDevice);
        group.MapPatch("/", UpdateDevice);
    }

    private static async Task<IResult> GetAllDevices(ApplicationDataContext db)
    {

        return Results.Ok();
    }

    private static async Task<IResult> AddDevice([FromBody] DeviceDto deviceDto, ApplicationDataContext db)
    {

        return Results.Ok();
    }

    private static async Task<IResult> DeleteDevice([FromQuery] int deviceId, ApplicationDataContext db)
    {

        return Results.Ok();
    }

    private static async Task<IResult> UpdateDevice([FromBody] DeviceDto device, ApplicationDataContext db)
    {

        return Results.Ok();
    }

    private record DeviceDto(string Data);
}