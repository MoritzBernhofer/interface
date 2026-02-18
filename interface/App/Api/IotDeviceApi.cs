using System.ComponentModel.DataAnnotations;
using App.Database;
using App.Database.Model.Iot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Api;

public static class IotDeviceApi
{
    public static void MapIotDeviceApi(this WebApplication app)
    {
        var group = app.MapGroup("/Devices")
            .WithTags("Devices")
            .WithOpenApi(o =>
            {
                o.Summary = "IoT device endpoints";
                o.Description = "Create, list, update and delete IoT devices.";
                return o;
            });

        group.MapGet("/", GetAllDevices)
            .WithName("GetAllDevices")
            .WithSummary("List all IoT devices")
            .WithDescription("Returns all registered IoT devices.")
            .Produces<IReadOnlyList<IotDevice>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();

        group.MapPost("/", AddDevice)
            .WithName("AddDevice")
            .WithSummary("Add a new IoT device")
            .WithDescription("Creates a new IoT device and returns it.")
            .Accepts<DeviceDto>("application/json")
            .Produces<IotDevice>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();

        group.MapDelete("/{deviceId:int}", DeleteDevice)
            .WithName("DeleteDevice")
            .WithSummary("Delete an IoT device")
            .WithDescription("Deletes a device by its id.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();

        group.MapPatch("/{deviceId:int}", UpdateDevice)
            .WithName("UpdateDevice")
            .WithSummary("Update an IoT device")
            .WithDescription("Partially updates an existing device by its id.")
            .Accepts<DeviceDto>("application/json")
            .Produces<IotDevice>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllDevices(ApplicationDataContext db)
    {
        var devices = await db.IotDevices.ToListAsync();
        return Results.Ok(devices);
    }

    private static async Task<IResult> AddDevice([FromBody] DeviceDto deviceDto, ApplicationDataContext db)
    {
        var device = new IotDevice
        {
            IPv4 = deviceDto.IPv4,
            Name = deviceDto.Name
        };

        db.IotDevices.Add(device);
        await db.SaveChangesAsync();

        return Results.Created($"/devices/{device.Id}", device);
    }

    private static async Task<IResult> DeleteDevice([FromRoute] int deviceId, ApplicationDataContext db)
    {
        var device = await db.IotDevices.FindAsync(deviceId);
        if (device is null)
            return Results.NotFound("Device not found");

        db.IotDevices.Remove(device);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateDevice([FromRoute] int deviceId, [FromBody] DeviceDto dto,
        ApplicationDataContext db)
    {
        var device = await db.IotDevices.FindAsync(deviceId);
        if (device is null)
            return Results.NotFound("Device not found");

        device.IPv4 = dto.IPv4;
        device.Name = dto.Name;

        await db.SaveChangesAsync();

        return Results.Ok(device);
    }

    private sealed class DeviceDto
    {
        [StringLength(15)] public required string IPv4 { get; set; }
        [StringLength(255)] public required string Name { get; set; }
    }
}