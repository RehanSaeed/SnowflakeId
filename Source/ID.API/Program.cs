using Boxed.AspNetCore;
using ID.API.Constants;
using ID.API.ViewModels;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SnowflakeIdOptions>(builder.Configuration.GetRequiredSection("SnowflakeId"));
builder.Services.AddSingleton<SnowflakeIdOptions>(x => x.GetRequiredService<IOptions<SnowflakeIdOptions>>().Value);
builder.Services.AddSingleton<SnowflakeIdService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

const int MaxCount = 100;

app.MapPost(
    "/snowflake-id",
    (HttpContext context, SnowflakeIdService snowflakeIdService, int? count) =>
    {
        if (!count.HasValue || count.Value <= 1)
        {
            return Results.Ok(new SnowflakeId(snowflakeIdService.CreateSnowflakeId()));
        }
        else if (count.Value > MaxCount)
        {
            var problemDetails = new HttpValidationProblemDetails(
                new Dictionary<string, string[]>()
                {
                    { nameof(count), new string[] { $"The field {nameof(count)} must be between 1 and {MaxCount}." } }
                })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            };
            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);
            return Results.BadRequest(problemDetails);
        }

        var snowflakeIds = new List<SnowflakeId>();
        for (int i = 0; i < count.Value; i++)
        {
            snowflakeIds.Add(new SnowflakeId(snowflakeIdService.CreateSnowflakeId()));
        }

        return Results.Ok(new SnowflakeIdItems(snowflakeIds));
    })
    .WithName(EndpointName.PostSnowflakeId)
    .Produces(StatusCodes.Status200OK, typeof(SnowflakeId))
    .Produces(StatusCodes.Status200OK, typeof(SnowflakeIdItems))
    .ProducesValidationProblem(StatusCodes.Status400BadRequest);

app.Run();
