using SQLite;

namespace LKBConvertor.Models
{
    public class ConversionHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string NomFichierSource { get; set; } = string.Empty;
        public string NomFichierSortie { get; set; } = string.Empty;
        public string CheminSortie { get; set; } = string.Empty;
        public int TypeConversion { get; set; }
        public string DateConversion { get; set; } = string.Empty;
        public long TailleOctets { get; set; }

        [Ignore]
        public string TailleAffichage => TailleOctets switch
        {
            < 1024 => $"{TailleOctets} B",
            < 1048576 => $"{TailleOctets / 1024.0:F1} KB",
            _ => $"{TailleOctets / 1048576.0:F1} MB"
        };

        [Ignore]
        public string EtiquetteType => (ConversionType)TypeConversion switch
        {
            Models.ConversionType.WordVersPdf       => "Word → PDF",
            Models.ConversionType.PdfVersRtf        => "PDF → RTF",
            Models.ConversionType.PdfVersWord       => "PDF → Word",
            Models.ConversionType.ImageVersPdf      => "Image → PDF",
            Models.ConversionType.ImageVersWord     => "Image → Word",
            Models.ConversionType.ExcelVersPdf      => "Excel → PDF",
            Models.ConversionType.PowerPointVersPdf => "PowerPoint → PDF",
            Models.ConversionType.PdfVersImage      => "PDF → Image",
            _ => "Inconnu"
        };

        [Ignore]
        public bool EstPdf =>
            Path.GetExtension(CheminSortie).Equals(".pdf",
                StringComparison.OrdinalIgnoreCase);
    }
}
