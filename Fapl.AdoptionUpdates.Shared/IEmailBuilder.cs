using Fapl.AdoptionUpdates.Shared.Helpers;
using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Fapl.AdoptionUpdates.Shared
{
    public interface IEmailBuilder<TModel>
    {
        string GenerateSubject(TModel model);
        string GenerateTextBody(TModel model);
        string GenerateHtmlBody(TModel model);
    }

    public static class IEmailBuilderExtensions
    {
        public static void Populate<TModel>(this IEmailBuilder<TModel> builder, MailMessage message, TModel model)
        {
            message.Subject = builder.GenerateSubject(model);

            string textBody = builder.GenerateTextBody(model);
            textBody = textBody.MultiLineTrim();
            textBody = textBody.Replace(Environment.NewLine, "\n");
            AlternateView textBodyView = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain");
            textBodyView.TransferEncoding = TransferEncoding.QuotedPrintable;
            message.AlternateViews.Add(textBodyView);

            string htmlBody = builder.GenerateHtmlBody(model);
            htmlBody = htmlBody.MultiLineTrim(removeEmptyLines: true);
            htmlBody = htmlBody.Replace(Environment.NewLine, "\n");
            AlternateView htmlBodyView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
            htmlBodyView.TransferEncoding = TransferEncoding.QuotedPrintable;
            message.AlternateViews.Add(htmlBodyView);
        }
    }
}
