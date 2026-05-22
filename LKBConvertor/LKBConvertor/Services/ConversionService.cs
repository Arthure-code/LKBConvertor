using LKBConvertor.Models;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;

namespace LKBConvertor.Services
{
    public class ConversionService
    {
        public async Task<ConversionResult> ConvertirWordVersPdf(
            string cheminFichier,
            Action<int> rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(20);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".pdf");

                    using (var docStream = File.OpenRead(cheminFichier))
                    using (var wordDoc = new WordDocument(
                        docStream, FormatType.Automatic))
                    {
                        rapportProgression?.Invoke(50);
                        using (var renderer = new DocIORenderer())
                        {
                            renderer.Settings.EmbedFonts = true;
                            using (var pdfDoc = renderer.ConvertToPDF(wordDoc))
                            {
                                rapportProgression?.Invoke(80);
                                using (var stream = new FileStream(
                                    cheminSortie, FileMode.Create))
                                {
                                    pdfDoc.Save(stream);
                                    pdfDoc.Close(true);
                                }
                            }
                        }
                    }

                    rapportProgression?.Invoke(100);
                    return new ConversionResult
                    {
                        EstSucces = true,
                        CheminSortie = cheminSortie,
                        TailleOctets = new FileInfo(cheminSortie).Length
                    };
                });
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    EstSucces = false,
                    MessageErreur = $"Erreur Word→PDF : {ex.Message}"
                };
            }
        }

        private string GenererCheminSortie(string cheminFichier, string ext)
        {
            var dossier = Path.Combine(
                FileSystem.CacheDirectory, "LKBConvertor");
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

        public async Task<ConversionResult> ConvertirPdfVersRtf(
    string cheminFichier,
    Action<int> rapportProgression = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    rapportProgression?.Invoke(20);
                    var cheminSortie = GenererCheminSortie(cheminFichier, ".rtf");

                    using (var pdfDoc = new PdfLoadedDocument(cheminFichier))
                    {
                        var sb = new System.Text.StringBuilder();
                        for (int i = 0; i < pdfDoc.Pages.Count; i++)
                        {
                            var page = pdfDoc.Pages[i] as PdfLoadedPage;
                            sb.AppendLine(page?.ExtractText() ?? string.Empty);
                            rapportProgression?.Invoke(20 + (int)(
                                i / (double)pdfDoc.Pages.Count * 60));
                        }

                        // RF04 : PDF scanné
                        if (sb.Length < 10)
                        {
                            pdfDoc.Close(true);
                            return new ConversionResult
                            {
                                EstSucces = false,
                                MessageErreur = "PDF scanné détecté. " +
                                    "Extraction de texte impossible."
                            };
                        }

                        var texte = sb.ToString()
                            .Replace("\\", "\\\\")
                            .Replace("{", "\\{")
                            .Replace("}", "\\}");

                        var rtf = "{\\rtf1\\ansi\\deff0 " +
                                  "{\\fonttbl{\\f0\\fswiss Arial;}}" +
                                  "\\f0\\fs22 " + texte + "}";

                        File.WriteAllText(cheminSortie, rtf,
                            System.Text.Encoding.ASCII);
                        pdfDoc.Close(true);
                    }

                    rapportProgression?.Invoke(100);
                    return new ConversionResult
                    {
                        EstSucces = true,
                        CheminSortie = cheminSortie,
                        TailleOctets = new FileInfo(cheminSortie).Length
                    };
                });
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    EstSucces = false,
                    MessageErreur = $"Erreur PDF→RTF : {ex.Message}"
                };
            }
        }
    }
}