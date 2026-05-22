using System.ComponentModel;
using LKBConvertor.Data;
using LKBConvertor.Models;

namespace LKBConvertor.ViewModels
{
    public class HistoriqueViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private LKBDatabase _bd = new LKBDatabase();

        public Command<ConversionHistory> SupprimerCommande { get; set; }
        public Command EffacerToutCommande { get; set; }
        public Command<ConversionHistory> PartagerCommande { get; set; }

        private List<ConversionHistory> _historique;
        public List<ConversionHistory> Historique
        {
            get { return _historique; }
            set
            {
                _historique = value;
                OnPropertyChanged(nameof(Historique));
                OnPropertyChanged(nameof(EstVide));
            }
        }

        public bool EstVide =>
            _historique == null || _historique.Count == 0;

        public HistoriqueViewModel()
        {
            SupprimerCommande = new Command<ConversionHistory>(Supprimer);
            EffacerToutCommande = new Command(EffacerTout);
            PartagerCommande = new Command<ConversionHistory>(Partager);
        }

        public void ChargerHistorique()
        {
            Historique = _bd.ObtenirHistorique();
        }

        private void Supprimer(ConversionHistory item)
        {
            if (item == null) return;
            _bd.Supprimer(item);
            ChargerHistorique();
        }

        private void EffacerTout()
        {
            _bd.EffacerTout();
            Historique = new List<ConversionHistory>();
        }

        private async void Partager(ConversionHistory item)
        {
            if (item == null) return;
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = item.NomFichierSortie,
                File = new ShareFile(item.CheminSortie)
            });
        }

        private void OnPropertyChanged(string nomPropriete)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nomPropriete));
        }
    }
}