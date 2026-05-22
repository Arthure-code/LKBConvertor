using System.ComponentModel;
using LKBConvertor.Data;
using LKBConvertor.Models;

namespace LKBConvertor.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private LKBDatabase _bd = new LKBDatabase();

        public Command NavigerWordPdfCommande { get; set; }
        public Command NavigerPdfRtfCommande { get; set; }
        public Command NavigerHistoriqueCommande { get; set; }

        private List<ConversionHistory> _conversionsRecentes;
        public List<ConversionHistory> ConversionsRecentes
        {
            get { return _conversionsRecentes; }
            set
            {
                _conversionsRecentes = value;
                OnPropertyChanged(nameof(ConversionsRecentes));
            }
        }

        public HomeViewModel()
        {
            NavigerWordPdfCommande = new Command(NavigerVersWordPdf);
            NavigerPdfRtfCommande = new Command(NavigerVersPdfRtf);
            NavigerHistoriqueCommande = new Command(NavigerVersHistorique);
        }

        private void NavigerVersWordPdf()
        {
            App.Current.MainPage.Navigation.PushAsync(
                new Views.ConversionPage(ConversionType.WordVersPdf));
        }

        private void NavigerVersPdfRtf()
        {
            App.Current.MainPage.Navigation.PushAsync(
                new Views.ConversionPage(ConversionType.PdfVersRtf));
        }

        private void NavigerVersHistorique()
        {
            App.Current.MainPage.Navigation.PushAsync(
                new Views.HistoriquePage());
        }

        public void ChargerConversionsRecentes()
        {
            ConversionsRecentes = _bd.ObtenirRecentes();
        }

        private void OnPropertyChanged(string nomPropriete)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nomPropriete));
        }
    }
}