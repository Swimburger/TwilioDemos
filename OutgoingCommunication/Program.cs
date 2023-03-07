using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using OptInStorage;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: false)
    .Build();

var optInNumbersRepository = new OptInRepository(config["OptInNumbersFile"]);
var phoneNumbers = optInNumbersRepository.GetOptIns();

var optInEmailRepository = new OptInRepository(config["OptInEmailsFile"]);
var emailAddresses = optInEmailRepository.GetOptIns();

var accountSid = config["Twilio:Client:AccountSid"];
var authToken = config["Twilio:Client:AuthToken"];
var fromPhoneNumber = config["Twilio:FromPhoneNumber"];
var fromName = config["SendGrid:FromName"];
var fromEmail = config["SendGrid:FromEmail"];

TwilioClient.Init(accountSid, authToken);

#region Messaging
Console.WriteLine("Press any key to send messages.");
Console.ReadKey();

foreach (var phoneNumber in phoneNumbers)
{
    var sms = await MessageResource.CreateAsync(
        to: new PhoneNumber(phoneNumber),
        from: new PhoneNumber(fromPhoneNumber),
        body: "Ahoy!"
    );

    Console.WriteLine($"Message SID: {sms.Sid}");
    Console.WriteLine($"Message Status: {sms.Status}");
}

#endregion

#region Calls
Console.WriteLine("Press any key to send calls.");
Console.ReadKey();

var calls = new List<CallResource>();
foreach (var phoneNumber in phoneNumbers)
{
    var call = await CallResource.CreateAsync(
        to: new PhoneNumber(phoneNumber),
        from: new PhoneNumber(fromPhoneNumber),
        url: new Uri("https://demo.twilio.com/docs/voice.xml")
    );

    calls.Add(call);
    
    Console.WriteLine($"Message SID: {call.Sid}");
    Console.WriteLine($"Message Status: {call.Status}");
}

Console.WriteLine("Press any key to update call");
Console.ReadKey();

foreach (var call in calls)
{
    try
    {
        await CallResource.UpdateAsync(
            twiml: new Twiml("<Response><Say>I am so sorry.</Say></Response>"),
            pathSid: call.Sid
        );
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

#endregion

#region Emails
Console.WriteLine("Press any key to send emails.");
Console.ReadKey();

var sendGridClient = new SendGridClient(config["SendGrid:ApiKey"]);
var email = new SendGridMessage
{

    From = new EmailAddress(fromEmail, fromName),
    Subject = "Greetings",
    HtmlContent = "Ahoy <b>matey</b>!"
};

foreach (var emailAddress in emailAddresses)
{
    var mailAddress = new MailAddress(emailAddress);
    email.AddTo(new EmailAddress(mailAddress.Address, mailAddress.DisplayName));
}

await sendGridClient.SendEmailAsync(email);

#endregion