using LKBConvertor.Models;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Presentation;
using Syncfusion.PresentationRenderer;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using DocFormat = Syncfusion.DocIO.FormatType;

namespace LKBConvertor.Services
{
    public static class ConversionService
    {
        // ---------- Word → PDF ----------
        public static async Task<ConversionResult> ConvertirWordVersPdf(
            string cheminFichier, Action<int>? rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(20);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".pdf");

                    using (var docStream = File.OpenRead(cheminFichier))
                    using (var wordDoc = new WordDocument(docStream, DocFormat.Automatic))
                    using (var renderer = new DocIORenderer())
                    {
                        renderer.Settings.EmbedFonts = true;
                        rapportProgression?.Invoke(50);
                        using var pdfDoc = renderer.ConvertToPDF(wordDoc);
                        rapportProgression?.Invoke(80);
                        using var stream = new FileStream(cheminSortie, FileMode.Create);
                        pdfDoc.Save(stream);
                    }

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur Word→PDF : {ex.Message}"); }
        }

        // ---------- PDF → RTF / Word ----------
        public static Task<ConversionResult> ConvertirPdfVersRtf(
            string cheminFichier, Action<int>? rapportProgression = null) =>
            PdfVersDocIO(cheminFichier, ".rtf", DocFormat.Rtf, "PDF→RTF", rapportProgression);

        public static Task<ConversionResult> ConvertirPdfVersWord(
            string cheminFichier, Action<int>? rapportProgression = null) =>
            PdfVersDocIO(cheminFichier, ".docx", DocFormat.Docx, "PDF→Word", rapportProgression);

        private static async Task<ConversionResult> PdfVersDocIO(
            string cheminFichier, string extension, DocFormat format,
            string libelle, Action<int>? rapportProgression)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(20);
                    var cheminSortie = GenererCheminSortie(cheminFichier, extension);

                    string[] pages;
                    using (var pdfDoc = new PdfLoadedDocument(cheminFichier))
                    {
                        var nbPages = pdfDoc.Pages.Count;
                        if (nbPages == 0) return Echec("PDF vide ou illisible.");

                        pages = new string[nbPages];
                        for (int i = 0; i < nbPages; i++)
                        {
                            var page = pdfDoc.Pages[i] as PdfLoadedPage;
                            pages[i] = page?.ExtractText() ?? string.Empty;
                            rapportProgression?.Invoke(20 + (int)(i / (double)nbPages * 50));
                        }
                    }

                    if (string.Join("\n", pages).Trim().Length < 10)
                        return Echec("PDF scanné détecté. Extraction de texte impossible.");

                    rapportProgression?.Invoke(80);
                    using (var wordDoc = new WordDocument())
                    {
                        var section = wordDoc.AddSection();
                        foreach (var contenuPage in pages)
                            foreach (var ligne in contenuPage.Split('\n'))
                                section.AddParagraph().AppendText(ligne);
                        using var flux = new FileStream(cheminSortie, FileMode.Create);
                        wordDoc.Save(flux, format);
                    }

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur {libelle} : {ex.Message}"); }
        }

        // ---------- Image → PDF ----------
        public static async Task<ConversionResult> ConvertirImageVersPdf(
            string cheminFichier, Action<int>? rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(30);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".pdf");

                    using (var pdfDoc = new PdfDocument())
                    {
                        var page = pdfDoc.Pages.Add();
                        rapportProgression?.Invoke(50);

                        using var imageStream = File.OpenRead(cheminFichier);
                        var image = PdfImage.FromStream(imageStream);
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

                        rapportProgression?.Invoke(80);
                        using var flux = new FileStream(cheminSortie, FileMode.Create);
                        pdfDoc.Save(flux);
                    }

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur Image→PDF : {ex.Message}"); }
        }

        // ---------- Image → Word ----------
        public static async Task<ConversionResult> ConvertirImageVersWord(
            string cheminFichier, Action<int>? rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(30);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".docx");

                    using (var wordDoc = new WordDocument())
                    {
                        var section = wordDoc.AddSection();
                        var paragraphe = section.AddParagraph();
                        using var imageStream = File.OpenRead(cheminFichier);
                        paragraphe.AppendPicture(imageStream);

                        rapportProgression?.Invoke(80);
                        using var flux = new FileStream(cheminSortie, FileMode.Create);
                        wordDoc.Save(flux, DocFormat.Docx);
                    }

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur Image→Word : {ex.Message}"); }
        }

        // ---------- Excel → PDF ----------
        public static async Task<ConversionResult> ConvertirExcelVersPdf(
            string cheminFichier, Action<int>? rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(20);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".pdf");

                    using var excelEngine = new ExcelEngine();
                    var application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Xlsx;

                    using var stream = File.OpenRead(cheminFichier);
                    var workbook = application.Workbooks.Open(stream);
                    rapportProgression?.Invoke(50);

                    var renderer = new XlsIORenderer();
                    using var pdfDoc = renderer.ConvertToPDF(workbook);
                    rapportProgression?.Invoke(80);

                    using var flux = new FileStream(cheminSortie, FileMode.Create);
                    pdfDoc.Save(flux);

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur Excel→PDF : {ex.Message}"); }
        }

        // ---------- PowerPoint → PDF ----------
        public static async Task<ConversionResult> ConvertirPowerPointVersPdf(
            string cheminFichier, Action<int>? rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(20);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".pdf");

                    using var stream = File.OpenRead(cheminFichier);
                    using var presentation = Presentation.Open(stream);
                    rapportProgression?.Invoke(50);

                    using var pdfDoc = PresentationToPdfConverter.Convert(presentation);
                    rapportProgression?.Invoke(80);

                    using var flux = new FileStream(cheminSortie, FileMode.Create);
                    pdfDoc.Save(flux);

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur PowerPoint→PDF : {ex.Message}"); }
        }

        // ---------- PDF → Image (Android natif) ----------
        public static async Task<ConversionResult> ConvertirPdfVersImage(
            string cheminFichier, Action<int>? rapportProgression = null)
        {
#if ANDROID
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(30);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".png");

                    var javaFile = new Java.IO.File(cheminFichier);
                    using var fd = Android.OS.ParcelFileDescriptor.Open(
                        javaFile, Android.OS.ParcelFileMode.ReadOnly);
                    if (fd == null)
                        return Echec("Impossible d'ouvrir le fichier PDF.");
                    using var renderer = new Android.Graphics.Pdf.PdfRenderer(fd);
                    if (renderer.PageCount == 0)
                        return Echec("PDF vide.");

                    using var page = renderer.OpenPage(0);
                    var bitmap = Android.Graphics.Bitmap.CreateBitmap(
                        page.Width * 2, page.Height * 2,
                        Android.Graphics.Bitmap.Config.Argb8888!);
                    page.Render(bitmap, null, null,
                        Android.Graphics.Pdf.PdfRenderMode.ForDisplay);

                    rapportProgression?.Invoke(80);
                    using (var fs = new FileStream(cheminSortie, FileMode.Create))
                    {
                        bitmap.Compress(
                            Android.Graphics.Bitmap.CompressFormat.Png!, 100, fs);
                    }
                    bitmap.Recycle();

                    rapportProgression?.Invoke(100);
                    return Reussi(cheminSortie);
                });
            }
            catch (Exception ex) { return Echec($"Erreur PDF→Image : {ex.Message}"); }
#else
            await Task.CompletedTask;
            return Echec("PDF → Image disponible uniquement sur Android.");
#endif
        }

        // ---------- Helpers ----------
        private static ConversionResult Reussi(string chemin) => new()
        {
            EstSucces = true,
            CheminSortie = chemin,
            TailleOctets = new FileInfo(chemin).Length
        };

        private static ConversionResult Echec(string message) => new()
        {
            EstSucces = false,
            MessageErreur = message
        };

        private static string GenererCheminSortie(string cheminFichier, string ext)
        {
            var dossier = Path.Combine(FileSystem.CacheDirectory, "LKBConvertor");
            Directory.CreateDirectory(dossier);

            var nomBase = Path.GetFileNameWithoutExtension(cheminFichier);
            var candidat = Path.Combine(dossier, $"{nomBase}_converti{ext}");

            int compteur = 1;
            while (File.Exists(candidat))
            {
                candidat = Path.Combine(dossier,
                    $"{nomBase}_converti_{compteur}{ext}");
                compteur++;
            }
            return candidat;
        }
    }
}
