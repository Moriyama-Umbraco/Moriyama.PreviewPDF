using System.Drawing.Imaging;
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
using WebSupergoo.ABCpdf13;

namespace Moriyama.PreviewPDF.ABCpdf.NotificationHandlers
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
                                Doc doc = new Doc();

                                _logger.LogInformation("Getting PDF from file manager");
                                doc.Read(_mediaFileManager.GetFile(entity, out _));

                                doc.PageNumber = 1;
                                doc.Rect.String = doc.CropBox.String;
                                using (var memoryStream = new MemoryStream())
                                {
                                    _logger.LogInformation("Rendering page 1 of PDF");
                                    var thumbnail = doc.Rendering.GetBitmap();

                                    _logger.LogInformation("Creating PNG thumbnail and saving to memory stream");
                                    thumbnail.Save(memoryStream, ImageFormat.Png);
                                    memoryStream.Position = 0;

                                    _logger.LogInformation("Updating entity with thumbnail before saving");
                                    entity.SetValue(_mediaFileManager, _mediaUrlGeneratorCollection, _shortStringHelper, _contentTypeBaseServiceProvider, _configuration.ThumbnailAlias, _configuration.ThumbnailFileName, memoryStream);
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
