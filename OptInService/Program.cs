using System.Net.Mail;
using Microsoft.AspNetCore.HttpOverrides;
using OptInStorage;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail;
using Twilio.AspNet.Core;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new OptInNumbersRepository(builder.Configuration["OptInNumbersFile"]));
builder.Services.AddSingleton(new OptInEmailsRepository(builder.Configuration["OptInEmailsFile"]));

builder.Services.AddTwilioRequestValidation();
builder.Services.AddSendGrid(options => options.ApiKey = builder.Configuration["SendGrid:ApiKey"]);

builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

var app = builder.Build();

app.UseForwardedHeaders();

app.MapPost("/message", async (
    HttpRequest request,
    CancellationToken ct,
    OptInNumbersRepository optInRepository
) =>
{
    var form = await request.ReadFormAsync(ct).ConfigureAwait(false);
    var body = form["Body"].ToString();
    body = body.Replace("-", "").Replace(" ", "");
    if (body.Equals("optin", StringComparison.OrdinalIgnoreCase))
    {
        optInRepository.OptIn(form["From"].ToString());
        return new MessagingResponse()
            .Message("Opt-in successful.")
            .ToTwiMLResult();
    }

    return new MessagingResponse()
        .Message("Respond with 'opt in' to participate in the demos.")
        .ToTwiMLResult();
});


app.MapPost("/email", async (
    HttpRequest request,
    CancellationToken ct,
    OptInEmailsRepository optInRepository,
    ISendGridClient sendGridClient
) =>
{
    var form = await request.ReadFormAsync(ct).ConfigureAwait(false);
    var receivedFrom = form["from"].ToString();
    var subject = form["subject"].ToString();

    var headers = form["headers"].ToString()
        .Split("\n")
        .Where(h => h != "")
        .ToLookup(h => h.Split(':')[0], h => h.Split(": ")[1]);
   
    var messageSid =  headers["Message-ID"].First();
    subject = subject.Replace("-", "").Replace(" ", "");
    optInRepository.OptIn(form["from"].ToString());
    var message = new SendGridMessage
    {
        From = new EmailAddress(
            app.Configuration["SendGrid:FromEmail"],
            app.Configuration["SendGrid:FromName"]
        ),
        Subject = $"Re: {subject}"
    };
    
    var receivedFromAddress = new MailAddress(receivedFrom);
    message.Personalizations = new List<Personalization>{new()
    {
        Tos = new List<EmailAddress> {new(receivedFromAddress.Address, receivedFromAddress.DisplayName)},
        Headers = new Dictionary<string, string>
        {
            {"in_reply_to", messageSid},
            {"References", messageSid}
        }
    }};

    message.PlainTextContent = subject.Equals("optin", StringComparison.OrdinalIgnoreCase)
        ? "Opt-in successful."
        : "Send email with subject 'opt in' to participate in the demos.";
    
    await sendGridClient.SendEmailAsync(message).ConfigureAwait(false);
});
    
app.Run();