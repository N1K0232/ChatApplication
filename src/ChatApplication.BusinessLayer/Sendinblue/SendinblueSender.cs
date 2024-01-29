using ChatApplication.BusinessLayer.Settings;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;

namespace ChatApplication.BusinessLayer.Sendinblue;

public class SendinblueSender : ISender
{
    private readonly SendinblueSettings sendinblueSettings;

    public SendinblueSender(IOptions<SendinblueSettings> sendinblueSettingsOptions)
    {
        sendinblueSettings = sendinblueSettingsOptions.Value;
    }

    public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
        => SendAsync(email, token).GetAwaiter().GetResult();

    public async Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
    {
        var response = new SendResponse();

        try
        {
            var result = await SendEmailAsync(email);
            if (result.StatusCode is >= 200 and <= 299)
            {
                response.MessageId = result.Data.MessageId;
            }
            else
            {
                response.ErrorMessages.Add(result.StatusCode.ToString());
            }
        }
        catch (Exception ex)
        {
            response.ErrorMessages.Add(ex.Message);
        }

        return response;
    }

    private async Task<ApiResponse<CreateSmtpEmail>> SendEmailAsync(IFluentEmail fluentEmail)
    {
        var emailSender = new TransactionalEmailsApi();
        var sendSmtpEmail = await CreateSmtpEmailAsync(fluentEmail);

        var sendResult = await emailSender.SendTransacEmailAsyncWithHttpInfo(sendSmtpEmail);
        return sendResult;
    }

    private async Task<SendSmtpEmailAttachment> ConvertAttachmentAsync(Attachment attachment)
    {
        var stream = attachment.Data;
        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);
        var content = memoryStream.ToArray();

        var sendinblueAttachment = new SendSmtpEmailAttachment
        {
            Content = content,
            Name = attachment.Filename
        };

        return sendinblueAttachment;
    }

    private async Task<SendSmtpEmail> CreateSmtpEmailAsync(IFluentEmail fluentEmail)
    {
        var content = fluentEmail.Data;
        var userName = !string.IsNullOrWhiteSpace(content.FromAddress.Name) ? content.FromAddress.Name : null;

        var emailAddress = content.FromAddress.EmailAddress;
        var smtpSender = new SendSmtpEmailSender(userName, emailAddress);

        var to = content.ToAddresses.Any() ? content.ToAddresses.Select(a => new SendSmtpEmailTo(a.EmailAddress, a.Name)).ToList() : null;
        var cc = content.CcAddresses.Any() ? content.CcAddresses.Select(a => new SendSmtpEmailCc(a.EmailAddress, a.Name)).ToList() : null;
        var bcc = content.BccAddresses.Any() ? content.BccAddresses.Select(a => new SendSmtpEmailBcc(a.EmailAddress, a.Name)).ToList() : null;

        var replyToAddress = content.ReplyToAddresses.Any() ? content.ReplyToAddresses.First() : null;
        var replyTo = replyToAddress != null ? new SendSmtpEmailReplyTo(replyToAddress.EmailAddress, replyToAddress.Name) : null;

        var sendSmtpEmail = new SendSmtpEmail(smtpSender, to, bcc, cc, replyTo: replyTo)
        {
            Subject = content.Subject
        };

        if (content.IsHtml)
        {
            sendSmtpEmail.HtmlContent = content.Body;
            sendSmtpEmail.TextContent = content.PlaintextAlternativeBody;
        }
        else
        {
            sendSmtpEmail.TextContent = content.Body;
        }

        if (content.Attachments.Any())
        {
            sendSmtpEmail.Attachment = [];
            foreach (var attachment in content.Attachments)
            {
                var emailAttachment = await ConvertAttachmentAsync(attachment);
                sendSmtpEmail.Attachment.Add(emailAttachment);
            }
        }

        return sendSmtpEmail;
    }
}