using Microsoft.AspNetCore.HttpOverrides;
using Twilio.AspNet.Core;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTwilioClient()
    .AddTwilioRequestValidation();

builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

var app = builder.Build();

app.UseForwardedHeaders();

var twilioEndpoints = app.MapGroup("");
twilioEndpoints.ValidateTwilioRequest();

twilioEndpoints.MapPost("/message", async (
    HttpRequest request,
    CancellationToken ct
) =>
{
    var form = await request.ReadFormAsync(ct).ConfigureAwait(false);
    var body = form["Body"];

    return new MessagingResponse()
        .Message($"You said: {body}")
        .ToTwiMLResult();
});

twilioEndpoints.MapPost("/voice", () => new VoiceResponse()
    .Say("Ahoy!")
    .ToTwiMLResult()
);

app.Run();
