namespace Moriyama.PreviewPDF.Ghostscript.Models
{
    public class MoriyamaPdfPreviewConfiguration
    {
        public int PdfPageNumber { get; set; } = 1;

        public string PdfFileAlias { get; set; } = "umbracoFile";

        public string ThumbnailAlias { get; set; } = "thumbnail";

        public string ThumbnailFileName { get; set; } = "thumbnail.png";
    }
}
