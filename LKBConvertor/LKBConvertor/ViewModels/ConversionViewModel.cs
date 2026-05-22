using System.ComponentModel;
using LKBConvertor.Data;
using LKBConvertor.Models;
using LKBConvertor.Services;

namespace LKBConvertor.ViewModels
{
    public class ConversionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private LKBDatabase _bd = new LKBDatabase();
        private ConversionService _service = new ConversionService();
        private ConversionType _typeConversion;

        public Command ChoisirFichierCommande { get; set; }
        public Command ConvertirCommande { get; set; }
        public Command PartagerCommande { get; set; }
        public Command ReinitialisierCommande { get; set; }
        public Command OuvrirVisionneuse { get; set; }

        private string _cheminFichier = string.Empty;
        public string CheminFichier
        {
            get { return _cheminFichier; }
            set
            {
                _cheminFichier = value;
                OnPropertyChanged(nameof(CheminFichier));
                OnPropertyChanged(nameof(FichierSelectionne));
                OnPropertyChanged(nameof(NomFichier));
            }
        }

        private int _progression = 0;
        public int Progression
        {
            get { return _progression; }
            set { _progression = value; OnPropertyChanged(nameof(Progression)); }
        }

        private bool _enCours = false;
        public bool EnCours
        {
            get { return _enCours; }
            set { _enCours = value; OnPropertyChanged(nameof(EnCours)); }
        }

        private bool _estSucces = false;
        public bool EstSucces
        {
            get { return _estSucces; }
            set { _estSucces = value; OnPropertyChanged(nameof(EstSucces)); }
        }

        private string _cheminSortie = string.Empty;
        public string CheminSortie
        {
            get { return _cheminSortie; }
            set { _cheminSortie = value; OnPropertyChanged(nameof(CheminSortie)); }
        }

        private string _messageErreur = string.Empty;
        public string MessageErreur
        {
            get { return _messageErreur; }
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

        public ConversionViewModel(ConversionType type)
        {
            _typeConversion = type;
            ChoisirFichierCommande = new Command(ChoisirFichier);
            ConvertirCommande = new Command(Convertir, PeutConvertir);
            PartagerCommande = new Command(Partager);
            ReinitialisierCommande = new Command(Reinitialiser);
            OuvrirVisionneuse = new Command(OuvrirViewer);
        }

        private bool PeutConvertir() => FichierSelectionne && !EnCours;

        private async void ChoisirFichier()
        {
            var extensions = _typeConversion == ConversionType.WordVersPdf
                ? new[] { ".docx", ".doc" }
                : new[] { ".pdf" };

            var types = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, extensions },
                    { DevicePlatform.iOS,     extensions }
                });

            var resultat = await FilePicker.Default.PickAsync(
                new PickOptions { FileTypes = types });

            if (resultat != null)
            {
                CheminFichier = resultat.FullPath;
                MessageErreur = string.Empty;
                EstSucces = false;
            }
        }

        private async void Convertir()
        {
            if (new FileInfo(CheminFichier).Length > 52_428_800)
            {
                bool continuer = await App.Current.MainPage.DisplayAlert(
                    "Fichier volumineux",
                    "Ce fichier dépasse 50 MB. La conversion peut être lente.",
                    "Continuer", "Annuler");
                if (!continuer) return;
            }

            EnCours = true;
            Progression = 0;
            MessageErreur = string.Empty;

            ConversionResult resultat;

            if (_typeConversion == ConversionType.WordVersPdf)
            {
                resultat = await _service.ConvertirWordVersPdf(
                    CheminFichier, v => Progression = v);
            }
            else
            {
                resultat = await _service.ConvertirPdfVersRtf(
                    CheminFichier, v => Progression = v);
            }

            EnCours = false;

            if (resultat.EstSucces)
            {
                CheminSortie = resultat.CheminSortie;
                EstSucces = true;

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
                MessageErreur = resultat.MessageErreur;
            }
        }

        private async void Partager()
        {
            if (string.IsNullOrEmpty(CheminSortie)) return;
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = Path.GetFileName(CheminSortie),
                File = new ShareFile(CheminSortie)
            });
        }

        private void OuvrirViewer()
        {
            App.Current.MainPage.Navigation.PushAsync(
                new Views.PdfViewerPage(CheminSortie));
        }

        private void Reinitialiser()
        {
            CheminFichier = string.Empty;
            CheminSortie = string.Empty;
            EstSucces = false;
            MessageErreur = string.Empty;
            Progression = 0;
        }

        private void OnPropertyChanged(string nomPropriete)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nomPropriete));
        }
    }
}