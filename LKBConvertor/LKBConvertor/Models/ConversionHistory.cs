using SQLite;

namespace LKBConvertor.Models
{
    public class ConversionHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string NomFichierSource { get; set; }
        public string NomFichierSortie { get; set; }
        public string CheminSortie { get; set; }
        public int TypeConversion { get; set; }
        public string DateConversion { get; set; }
        public long TailleOctets { get; set; }

        [Ignore]
        public string TailleAffichage
        {
            get
            {
                if (TailleOctets < 1024)
                    return $"{TailleOctets} B";
                if (TailleOctets < 1048576)
                    return $"{TailleOctets / 1024.0:F1} KB";
                return $"{TailleOctets / 1048576.0:F1} MB";
            }
        }

        [Ignore]
        public string EtiquetteType
        {
            get
            {
                switch (TypeConversion)
                {
                    case 0: return "Word → PDF";
                    case 1: return "PDF → RTF";
                    default: return "Inconnu";
                }
            }
        }
    }
}