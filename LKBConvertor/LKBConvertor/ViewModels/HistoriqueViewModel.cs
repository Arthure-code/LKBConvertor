using System.ComponentModel;
using LKBConvertor.Data;
using LKBConvertor.Models;
using LKBConvertor.Services;

namespace LKBConvertor.ViewModels
{
    public class HistoriqueViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly LKBDatabase _bd;
        private readonly INavigationService _navigation;
        private readonly Func<string, Views.PdfViewerPage> _pdfViewerFactory;

        public Command<ConversionHistory> MenuCommande { get; }
        public Command EffacerToutCommande { get; }

        private List<ConversionHistory> _historique = new();
        public List<ConversionHistory> Historique
        {
            get => _historique;
            set
            {
                _historique = value;
                OnPropertyChanged(nameof(Historique));
                OnPropertyChanged(nameof(EstVide));
            }
        }

        public bool EstVide => _historique == null || _historique.Count == 0;

        public HistoriqueViewModel(
            LKBDatabase bd,
            INavigationService navigation,
            Func<string, Views.PdfViewerPage> pdfViewerFactory)
        {
            _bd = bd;
            _navigation = navigation;
            _pdfViewerFactory = pdfViewerFactory;

            MenuCommande = new Command<ConversionHistory>(
                async item => await AfficherMenuAsync(item));
            EffacerToutCommande = new Command(async () => await EffacerToutAsync());
        }

        public void ChargerHistorique() => Historique = _bd.ObtenirHistorique();

        private async Task AfficherMenuAsync(ConversionHistory item)
        {
            if (item == null) return;

            var options = new List<string> { "Partager" };
            if (File.Exists(item.CheminSortie))
                options.Insert(0, "Ouvrir");

            var choix = await _navigation.AfficherOptions(
                item.NomFichierSortie,
                "Annuler",
                "Supprimer",
                options.ToArray());

            switch (choix)
            {
                case "Ouvrir":    await OuvrirAsync(item); break;
                case "Partager":  await PartagerAsync(item); break;
                case "Supprimer": Supprimer(item); break;
            }
        }

        private async Task EffacerToutAsync()
        {
            var confirmer = await _navigation.AfficherAlerte(
                "Effacer tout",
                "Supprimer toutes les conversions de l'historique ?",
                "Effacer", "Annuler");
            if (!confirmer) return;

            _bd.EffacerTout();
            Historique = new List<ConversionHistory>();
        }

        private void Supprimer(ConversionHistory item)
        {
            if (item == null) return;
            _bd.Supprimer(item);
            ChargerHistorique();
        }

        private static async Task PartagerAsync(ConversionHistory item)
        {
            if (item == null) return;
            try
            {
                await ShareHelper.PartagerFichierAsync(item.CheminSortie, item.NomFichierSortie);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Partage annulé ou échoué : {ex.Message}");
            }
        }

        private async Task OuvrirAsync(ConversionHistory item)
        {
            if (item == null || !File.Exists(item.CheminSortie)) return;
            try
            {
                await _navigation.PushAsync(_pdfViewerFactory(item.CheminSortie));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Navigation vers visionneuse échouée : {ex.Message}");
            }
        }

        private void OnPropertyChanged(string nomPropriete) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomPropriete));
    }
}
