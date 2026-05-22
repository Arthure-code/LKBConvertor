using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf.Parsing;
using LKBConvertor.Models;

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
    }
}