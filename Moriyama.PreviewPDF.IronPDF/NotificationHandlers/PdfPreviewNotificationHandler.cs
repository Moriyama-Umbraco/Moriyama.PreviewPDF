using IronSoftware.Drawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moriyama.PreviewPDF.Shared;
using Moriyama.PreviewPDF.Shared.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Moriyama.PreviewPDF.IronPDF.NotificationHandlers
{
    public class PdfPreviewNotificationHandler : INotificationHandler<MediaSavingNotification>
    {
        private readonly MediaFileManager _mediaFileManager;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly MediaUrlGeneratorCollection _mediaUrlGeneratorCollection;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly ILogger<PdfPreviewNotificationHandler> _logger;
        private readonly MoriyamaPdfPreviewConfiguration _configuration;

        public PdfPreviewNotificationHandler(MediaFileManager mediaFileManager, IShortStringHelper shortStringHelper, MediaUrlGeneratorCollection mediaUrlGeneratorCollection, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, ILogger<PdfPreviewNotificationHandler> logger, IWebHostEnvironment hostingEnvironment, IOptions<MoriyamaPdfPreviewConfiguration> options)
        {
            _mediaFileManager = mediaFileManager;
            _shortStringHelper = shortStringHelper;
            _mediaUrlGeneratorCollection = mediaUrlGeneratorCollection;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _logger = logger;
            _configuration = options.Value;
        }

        public void Handle(MediaSavingNotification notification)
        {
            foreach (var entity in notification.SavedEntities)
            {
                if (entity.ContentType.Alias.Equals(Umbraco.Cms.Core.Constants.Conventions.MediaTypes.ArticleAlias))
                {
                    var filePropertyValue = (string?)entity.GetValue(_configuration.PdfFileAlias);
                    if (!string.IsNullOrWhiteSpace(filePropertyValue))
                    {
                        var isPdf = filePropertyValue.EndsWith(MoriyamaPreviewPDFConstants.PdfExtension, StringComparison.InvariantCultureIgnoreCase);

                        if (isPdf)
                        {
                            try
                            {
                                _logger.LogInformation("Starting PDF thumbnail creation");
                                _logger.LogInformation("Getting PDF from file manager");

                                using (var pdf = new PdfDocument(_mediaFileManager.GetFile(entity, out _)))
                                {

                                    using (var memoryStream = new MemoryStream())
                                    {
                                        _logger.LogInformation("Rendering page 1 of PDF and creating PNG thumbnail and saving to memory stream");
                                        pdf.PageToBitmap(1).ExportStream(memoryStream, AnyBitmap.ImageFormat.Png);
                                        memoryStream.Position = 0;

                                        _logger.LogInformation("Updating entity with thumbnail before saving");
                                        entity.SetValue(_mediaFileManager, _mediaUrlGeneratorCollection, _shortStringHelper, _contentTypeBaseServiceProvider, _configuration.ThumbnailAlias, _configuration.ThumbnailFileName, memoryStream);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
