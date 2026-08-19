using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Presentation;
using Syncfusion.PresentationRenderer;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using DocFormat = Syncfusion.DocIO.FormatType;

namespace LKBConvertor.Views;

public partial class PdfViewerPage : ContentPage
{
    private readonly string _cheminFichier;
    private Stream? _stream;
    private string? _cheminPdfTemporaire;

    public PdfViewerPage(string cheminFichier)
    {
        InitializeComponent();
        _cheminFichier = cheminFichier;
        Title = Path.GetFileName(cheminFichier);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ChargerAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        pdfViewer.DocumentSource = null;
        _stream?.Dispose();
        _stream = null;

        if (_cheminPdfTemporaire != null && File.Exists(_cheminPdfTemporaire))
        {
            try
            {
                File.Delete(_cheminPdfTemporaire);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Cleanup PDF temporaire ignoré : {ex.Message}");
            }
            _cheminPdfTemporaire = null;
        }
    }

    private async Task ChargerAsync()
    {
        if (!File.Exists(_cheminFichier))
        {
            AfficherErreur("Fichier introuvable.");
            return;
        }

        var extension = Path.GetExtension(_cheminFichier).ToLowerInvariant();
        string cheminPdf;

        try
        {
            if (extension == ".pdf")
            {
                cheminPdf = _cheminFichier;
            }
            else if (extension is ".doc" or ".docx" or ".rtf" or ".odt" or ".txt")
            {
                chargementOverlay.IsVisible = true;
                cheminPdf = await ConvertirVersPdfAsync(_cheminFichier);
                _cheminPdfTemporaire = cheminPdf;
                chargementOverlay.IsVisible = false;
            }
            else if (extension is ".jpg" or ".jpeg" or ".png" or ".bmp")
            {
                chargementOverlay.IsVisible = true;
                cheminPdf = await ImageVersPdfAsync(_cheminFichier);
                _cheminPdfTemporaire = cheminPdf;
                chargementOverlay.IsVisible = false;
            }
            else if (extension is ".xlsx" or ".xls")
            {
                chargementOverlay.IsVisible = true;
                cheminPdf = await ExcelVersPdfAsync(_cheminFichier);
                _cheminPdfTemporaire = cheminPdf;
                chargementOverlay.IsVisible = false;
            }
            else if (extension is ".pptx" or ".ppt")
            {
                chargementOverlay.IsVisible = true;
                cheminPdf = await PowerPointVersPdfAsync(_cheminFichier);
                _cheminPdfTemporaire = cheminPdf;
                chargementOverlay.IsVisible = false;
            }
            else
            {
                AfficherErreur($"Format « {extension} » non pris en charge.");
                return;
            }

            _stream = File.OpenRead(cheminPdf);
            pdfViewer.DocumentSource = _stream;
        }
        catch (Exception ex)
        {
            chargementOverlay.IsVisible = false;
            AfficherErreur($"Impossible d'ouvrir le document : {ex.Message}");
        }
    }

    private static Task<string> ExcelVersPdfAsync(string cheminSource) =>
        Task.Run(() =>
        {
            var cheminPdf = CheminPdfTemp(cheminSource);
            using var excelEngine = new ExcelEngine();
            excelEngine.Excel.DefaultVersion = ExcelVersion.Xlsx;
            using var stream = File.OpenRead(cheminSource);
            var workbook = excelEngine.Excel.Workbooks.Open(stream);
            var renderer = new XlsIORenderer();
            using var pdfDoc = renderer.ConvertToPDF(workbook);
            using var fs = new FileStream(cheminPdf, FileMode.Create);
            pdfDoc.Save(fs);
            return cheminPdf;
        });

    private static Task<string> PowerPointVersPdfAsync(string cheminSource) =>
        Task.Run(() =>
        {
            var cheminPdf = CheminPdfTemp(cheminSource);
            using var stream = File.OpenRead(cheminSource);
            using var presentation = Presentation.Open(stream);
            using var pdfDoc = PresentationToPdfConverter.Convert(presentation);
            using var fs = new FileStream(cheminPdf, FileMode.Create);
            pdfDoc.Save(fs);
            return cheminPdf;
        });

    private static string CheminPdfTemp(string cheminSource)
    {
        var dossier = Path.Combine(FileSystem.CacheDirectory, "LKBConvertor", "viewer");
        Directory.CreateDirectory(dossier);
        return Path.Combine(dossier,
            Path.GetFileNameWithoutExtension(cheminSource) + "_view.pdf");
    }

    private static Task<string> ImageVersPdfAsync(string cheminSource) =>
        Task.Run(() =>
        {
            var dossier = Path.Combine(FileSystem.CacheDirectory, "LKBConvertor", "viewer");
            Directory.CreateDirectory(dossier);
            var cheminPdf = Path.Combine(dossier,
                Path.GetFileNameWithoutExtension(cheminSource) + "_view.pdf");

            using var pdfDoc = new PdfDocument();
            var page = pdfDoc.Pages.Add();
            using var imgStream = File.OpenRead(cheminSource);
            var image = PdfImage.FromStream(imgStream);
            var tailleClient = page.GetClientSize();
            var ratio = Math.Min(
                tailleClient.Width / image.Width,
                tailleClient.Height / image.Height);
            var largeur = image.Width * ratio;
            var hauteur = image.Height * ratio;
            page.Graphics.DrawImage(image,
                (tailleClient.Width - largeur) / 2,
                (tailleClient.Height - hauteur) / 2,
                largeur, hauteur);

            using var pdfStream = new FileStream(cheminPdf, FileMode.Create);
            pdfDoc.Save(pdfStream);
            return cheminPdf;
        });

    private static Task<string> ConvertirVersPdfAsync(string cheminSource) =>
        Task.Run(() =>
        {
            var dossier = Path.Combine(FileSystem.CacheDirectory, "LKBConvertor", "viewer");
            Directory.CreateDirectory(dossier);
            var cheminPdf = Path.Combine(dossier,
                Path.GetFileNameWithoutExtension(cheminSource) + "_view.pdf");

            using var docStream = File.OpenRead(cheminSource);
            using var wordDoc = new WordDocument(docStream, DocFormat.Automatic);
            using var renderer = new DocIORenderer();
            renderer.Settings.EmbedFonts = true;
            using var pdfDoc = renderer.ConvertToPDF(wordDoc);
            using var pdfStream = new FileStream(cheminPdf, FileMode.Create);
            pdfDoc.Save(pdfStream);

            return cheminPdf;
        });

    private void AfficherErreur(string message)
    {
        pdfViewer.IsVisible = false;
        messageErreur.Text = message;
        messageErreur.IsVisible = true;
    }
}
