using System.ComponentModel;
using LKBConvertor.Data;
using LKBConvertor.Models;
using LKBConvertor.Services;

namespace LKBConvertor.ViewModels
{
    public class ConversionViewModel : INotifyPropertyChanged
    {
        // Constantes de mimes / UTIs / extensions par type — static readonly
        // pour éviter d'allouer les tableaux à chaque appel (S3887).
        private static readonly string[] UtisWord = { "org.openxmlformats.wordprocessingml.document", "com.microsoft.word.doc" };
        private static readonly string[] ExtsWord = { ".docx", ".doc" };
        private static readonly string[] UtisPdf = { "com.adobe.pdf" };
        private static readonly string[] ExtsPdf = { ".pdf" };
        private static readonly string[] UtisImage = { "public.image" };
        private static readonly string[] ExtsImage = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] UtisExcel = { "org.openxmlformats.spreadsheetml.sheet", "com.microsoft.excel.xls" };
        private static readonly string[] ExtsExcel = { ".xlsx", ".xls" };
        private static readonly string[] UtisPowerPoint = { "org.openxmlformats.presentationml.presentation", "com.microsoft.powerpoint.ppt" };
        private static readonly string[] ExtsPowerPoint = { ".pptx", ".ppt" };
        private static readonly string[] UtisDefault = { "public.data" };
        private static readonly string[] ExtsDefault = { ".*" };
        private static readonly string[] MimesAndroidWildcard = { "*/*" };

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly LKBDatabase _bd;
        private readonly INavigationService _navigation;
        private readonly Func<string, Views.PdfViewerPage> _pdfViewerFactory;
        private readonly ConversionType _typeConversion;

        public Command ChoisirFichierCommande { get; }
        public Command ConvertirCommande { get; }
        public Command PartagerCommande { get; }
        public Command ReinitialiserCommande { get; }
        public Command OuvrirVisionneuseCommande { get; }

        private string _cheminFichier = string.Empty;
        public string CheminFichier
        {
            get => _cheminFichier;
            set
            {
                _cheminFichier = value;
                OnPropertyChanged(nameof(CheminFichier));
                OnPropertyChanged(nameof(FichierSelectionne));
                OnPropertyChanged(nameof(NomFichier));
                ConvertirCommande.ChangeCanExecute();
            }
        }

        private double _progression;
        public double Progression
        {
            get => _progression;
            set { _progression = value; OnPropertyChanged(nameof(Progression)); }
        }

        private bool _enCours;
        public bool EnCours
        {
            get => _enCours;
            set
            {
                _enCours = value;
                OnPropertyChanged(nameof(EnCours));
                ConvertirCommande.ChangeCanExecute();
            }
        }

        private bool _estSucces;
        public bool EstSucces
        {
            get => _estSucces;
            set { _estSucces = value; OnPropertyChanged(nameof(EstSucces)); }
        }

        private string _cheminSortie = string.Empty;
        public string CheminSortie
        {
            get => _cheminSortie;
            set { _cheminSortie = value; OnPropertyChanged(nameof(CheminSortie)); }
        }

        private string _messageErreur = string.Empty;
        public string MessageErreur
        {
            get => _messageErreur;
            set
            {
                _messageErreur = value;
                OnPropertyChanged(nameof(MessageErreur));
                OnPropertyChanged(nameof(AErreur));
            }
        }

        public bool FichierSelectionne => !string.IsNullOrEmpty(_cheminFichier);
        public string NomFichier => string.IsNullOrEmpty(_cheminFichier)
            ? string.Empty : Path.GetFileName(_cheminFichier);
        public bool AErreur => !string.IsNullOrEmpty(_messageErreur);
        public bool PeutOuvrirVisionneuse => EstSucces;

        public ConversionViewModel(
            ConversionType type,
            LKBDatabase bd,
            INavigationService navigation,
            Func<string, Views.PdfViewerPage> pdfViewerFactory)
        {
            _typeConversion = type;
            _bd = bd;
            _navigation = navigation;
            _pdfViewerFactory = pdfViewerFactory;

            ChoisirFichierCommande = new Command(
                async () => await ExecuterAvecGarde(ChoisirFichierAsync));
            ConvertirCommande = new Command(
                async () => await ExecuterAvecGarde(ConvertirAsync), PeutConvertir);
            PartagerCommande = new Command(
                async () => await ExecuterAvecGarde(PartagerAsync));
            ReinitialiserCommande = new Command(Reinitialiser);
            OuvrirVisionneuseCommande = new Command(
                async () => await ExecuterAvecGarde(OuvrirViewerAsync));
        }

        private bool PeutConvertir() => FichierSelectionne && !EnCours;

        private async Task ExecuterAvecGarde(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                EnCours = false;
                MessageErreur = ex.Message;
            }
        }

        private async Task ChoisirFichierAsync()
        {
            var (utis, winExts, extensionsAttendues) = _typeConversion switch
            {
                ConversionType.WordVersPdf => (UtisWord, ExtsWord, ExtsWord),

                ConversionType.PdfVersRtf or ConversionType.PdfVersWord
                    or ConversionType.PdfVersImage => (UtisPdf, ExtsPdf, ExtsPdf),

                ConversionType.ImageVersPdf or ConversionType.ImageVersWord =>
                    (UtisImage, ExtsImage, ExtsImage),

                ConversionType.ExcelVersPdf => (UtisExcel, ExtsExcel, ExtsExcel),

                ConversionType.PowerPointVersPdf =>
                    (UtisPowerPoint, ExtsPowerPoint, ExtsPowerPoint),

                _ => (UtisDefault, ExtsDefault, Array.Empty<string>())
            };

            // Android : "*/*" pour éviter le grisage (beaucoup de fichiers arrivent
            // en application/octet-stream depuis Drive/Downloads). On valide après.
            var types = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, MimesAndroidWildcard },
                    { DevicePlatform.iOS,     utis },
                    { DevicePlatform.WinUI,   winExts }
                });

            var titrePicker = _typeConversion switch
            {
                ConversionType.WordVersPdf => "Choisir un document Word",
                ConversionType.PdfVersRtf or ConversionType.PdfVersWord
                    or ConversionType.PdfVersImage => "Choisir un document PDF",
                ConversionType.ImageVersPdf or ConversionType.ImageVersWord => "Choisir une image",
                ConversionType.ExcelVersPdf => "Choisir un classeur Excel",
                ConversionType.PowerPointVersPdf => "Choisir une présentation PowerPoint",
                _ => "Choisir un fichier"
            };

            var resultat = await FilePicker.Default.PickAsync(
                new PickOptions { FileTypes = types, PickerTitle = titrePicker });

            if (resultat == null) return;

            // Validation de l'extension
            var ext = Path.GetExtension(resultat.FileName ?? resultat.FullPath)
                          .ToLowerInvariant();
            if (extensionsAttendues.Length > 0 &&
                Array.IndexOf(extensionsAttendues, ext) < 0)
            {
                MessageErreur = $"Type de fichier non pris en charge. " +
                    $"Attendu : {string.Join(", ", extensionsAttendues)}";
                return;
            }

            CheminFichier = resultat.FullPath;
            MessageErreur = string.Empty;
            EstSucces = false;
            OnPropertyChanged(nameof(PeutOuvrirVisionneuse));
        }

        private async Task ConvertirAsync()
        {
            long taille;
            try
            {
                taille = new FileInfo(CheminFichier).Length;
            }
            catch (Exception ex)
            {
                MessageErreur = $"Fichier introuvable : {ex.Message}";
                return;
            }

            if (taille > 52_428_800)
            {
                bool continuer = await _navigation.AfficherAlerte(
                    "Fichier volumineux",
                    "Ce fichier dépasse 50 MB. La conversion peut être lente.",
                    "Continuer", "Annuler");
                if (!continuer) return;
            }

            EnCours = true;
            Progression = 0;
            MessageErreur = string.Empty;

            Action<int> progression = v =>
                MainThread.BeginInvokeOnMainThread(() => Progression = v / 100.0);

            ConversionResult resultat = _typeConversion switch
            {
                ConversionType.WordVersPdf       => await ConversionService.ConvertirWordVersPdf(CheminFichier, progression),
                ConversionType.PdfVersRtf        => await ConversionService.ConvertirPdfVersRtf(CheminFichier, progression),
                ConversionType.PdfVersWord       => await ConversionService.ConvertirPdfVersWord(CheminFichier, progression),
                ConversionType.ImageVersPdf      => await ConversionService.ConvertirImageVersPdf(CheminFichier, progression),
                ConversionType.ImageVersWord     => await ConversionService.ConvertirImageVersWord(CheminFichier, progression),
                ConversionType.ExcelVersPdf      => await ConversionService.ConvertirExcelVersPdf(CheminFichier, progression),
                ConversionType.PowerPointVersPdf => await ConversionService.ConvertirPowerPointVersPdf(CheminFichier, progression),
                ConversionType.PdfVersImage      => await ConversionService.ConvertirPdfVersImage(CheminFichier, progression),
                _ => new ConversionResult { EstSucces = false, MessageErreur = "Type de conversion non pris en charge." }
            };

            EnCours = false;

            if (resultat.EstSucces)
            {
                CheminSortie = resultat.CheminSortie ?? string.Empty;
                EstSucces = true;
                OnPropertyChanged(nameof(PeutOuvrirVisionneuse));

                _bd.Inserer(new ConversionHistory
                {
                    NomFichierSource = NomFichier,
                    NomFichierSortie = Path.GetFileName(CheminSortie),
                    CheminSortie = CheminSortie,
                    TypeConversion = (int)_typeConversion,
                    DateConversion = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    TailleOctets = resultat.TailleOctets
                });
            }
            else
            {
                MessageErreur = resultat.MessageErreur ?? "Erreur inconnue.";
            }
        }

        private async Task PartagerAsync()
        {
            await ShareHelper.PartagerFichierAsync(CheminSortie);
        }

        private async Task OuvrirViewerAsync()
        {
            if (string.IsNullOrEmpty(CheminSortie)) return;
            await _navigation.PushAsync(_pdfViewerFactory(CheminSortie));
        }

        private void Reinitialiser()
        {
            CheminFichier = string.Empty;
            CheminSortie = string.Empty;
            EstSucces = false;
            MessageErreur = string.Empty;
            Progression = 0;
            OnPropertyChanged(nameof(PeutOuvrirVisionneuse));
        }

        private void OnPropertyChanged(string nomPropriete) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomPropriete));
    }
}
