using System.Drawing.Imaging;
using System.Reflection;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moriyama.PreviewPDF.Ghostscript.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Moriyama.PreviewPDF.Ghostscript.NotificationHandlers
{
    public class PdfPreviewNotificationHandler : INotificationHandler<MediaSavingNotification>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
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
            _hostingEnvironment = hostingEnvironment;
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
                                var binPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                                //var binPath = _hostingEnvironment.ContentRootPath + @"\App_Plugins\Moriyama.PreviewPDF.Ghostscript\Ghostscript";
                                var gsDllPath = Path.Combine(binPath, Environment.Is64BitProcess ? MoriyamaPreviewPDFConstants.GhostscriptDll64 : MoriyamaPreviewPDFConstants.GhostscriptDll32);
                                var version = new GhostscriptVersionInfo(new Version(10, 02, 1), gsDllPath, string.Empty, GhostscriptLicense.GPL);

                                using (var rasterizer = new GhostscriptRasterizer())
                                {
                                    var file = _mediaFileManager.GetFile(entity, out _);
                                    rasterizer.Open(file, version, true);
                                    var firstPageAsImage = rasterizer.GetPage(200, _configuration.PdfPageNumber);

                                    using (var memoryStream = new MemoryStream())
                                    {
                                        _logger.LogInformation("Creating PNG thumbnail and saving to memory stream");
                                        firstPageAsImage.Save(memoryStream, ImageFormat.Png);
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
