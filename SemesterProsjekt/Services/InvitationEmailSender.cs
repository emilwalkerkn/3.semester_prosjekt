using System.Net.Mail;

namespace SemesterProsjekt.Services;

public class InvitationEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public InvitationEmailSender(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task SendInvitationAsync(
        string email,
        string organizationName,
        string invitationLink)
    {
        // Foreløpig støtter vi bare lokal utvikling.
        if (!_environment.IsDevelopment())
            throw new InvalidOperationException(
                "E-posttjenesten er kun konfigurert for utvikling.");

        var smtpHost = _configuration["Smtp:Host"]
                       ?? throw new InvalidOperationException(
                           "SMTP-vert mangler.");

        var smtpPort = int.Parse(
            _configuration["Smtp:Port"] ?? "1025");

        using var message = new MailMessage
        {
            From = new MailAddress("invitasjon@kriseportal.local"),
            Subject = $"Invitasjon fra {organizationName}",
            Body = $"""
                    Hei!

                    Du er invitert til å bli Behovsaktør for
                    {organizationName} i Kriseportalen.

                    Åpne denne lenken for å godta invitasjonen:

                    {invitationLink}

                    Lenken er gyldig i 24 timer og kan bare brukes én gang.

                    Hvis du ikke forventet denne invitasjonen,
                    kan du ignorere e-posten.

                    Hilsen
                    Kriseportalen
                    """,
            IsBodyHtml = false
        };

        message.To.Add(email);

        using var smtp = new SmtpClient(smtpHost, smtpPort);

        await smtp.SendMailAsync(message);
    }
    
    public async Task SendOrganizationAdminInvitationAsync(
        string email,
        string organizationName,
        string invitationLink)
    {
        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "E-posttjenesten er kun konfigurert for utvikling.");
        }

        var smtpHost = _configuration["Smtp:Host"]
                       ?? throw new InvalidOperationException(
                           "SMTP-vert mangler.");

        var smtpPort = int.Parse(
            _configuration["Smtp:Port"] ?? "1025");

        using var message = new MailMessage
        {
            From = new MailAddress("invitasjon@kriseportal.local"),
            Subject = $"Organisasjonen {organizationName} er godkjent",
            Body = $"""
                    Hei!

                    Søknaden for {organizationName} er godkjent
                    i Kriseportalen.

                    Du er invitert til å opprette den første
                    organisasjonsadministratorkontoen.

                    Åpne denne lenken for å registrere deg:

                    {invitationLink}

                    Lenken er gyldig i 24 timer og kan bare brukes én gang.

                    Hvis du ikke forventet denne invitasjonen,
                    kan du ignorere e-posten.

                    Hilsen
                    Kriseportalen
                    """,
            IsBodyHtml = false
        };

        message.To.Add(email);

        using var smtp = new SmtpClient(smtpHost, smtpPort);

        await smtp.SendMailAsync(message);
    }

}