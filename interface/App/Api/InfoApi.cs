using App.Database;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

public static class InfoApi
{
    public static void MapInfoApi(this WebApplication app)
    {
        var group = app.MapGroup("PostInfo");

        group.MapPost("/", PostInfo);
    }

    private static async Task<IResult> PostInfo([FromBody] InfoDto info, ApplicationDataContext db)
    {

        return Results.Ok();
    }

    private record InfoDto(string Data);
}