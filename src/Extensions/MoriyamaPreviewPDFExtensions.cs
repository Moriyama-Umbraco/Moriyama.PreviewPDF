using Microsoft.Extensions.DependencyInjection;
using Moriyama.PreviewPDF.Ghostscript.Models;
using Moriyama.PreviewPDF.Ghostscript.NotificationHandlers;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Moriyama.PreviewPDF.Ghostscript.Extensions
{
    public static class MoriyamaPreviewPDFExtensions
    {
        public static IUmbracoBuilder AddMoriyamaPreviewPDF(this IUmbracoBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.Configure<MoriyamaPdfPreviewConfiguration>(builder.Config.GetSection(MoriyamaPreviewPDFConstants.ConfigurationSection));
            builder.AddNotificationHandler<MediaSavingNotification, PdfPreviewNotificationHandler>();

            return builder;
        }
    }
}
