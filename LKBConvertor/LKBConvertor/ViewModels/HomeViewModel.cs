using System.ComponentModel;
using LKBConvertor.Data;
using LKBConvertor.Models;
using LKBConvertor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LKBConvertor.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly LKBDatabase _bd;
        private readonly INavigationService _navigation;
        private readonly IServiceProvider _sp;
        private readonly Func<ConversionType, Views.ConversionPage> _conversionPageFactory;

        public Command<ConversionType> NavigerConversionCommande { get; }
        public Command NavigerHistoriqueCommande { get; }

        private List<ConversionHistory> _conversionsRecentes = new();
        public List<ConversionHistory> ConversionsRecentes
        {
            get => _conversionsRecentes;
            set
            {
                _conversionsRecentes = value;
                OnPropertyChanged(nameof(ConversionsRecentes));
            }
        }

        public HomeViewModel(
            LKBDatabase bd,
            INavigationService navigation,
            IServiceProvider sp,
            Func<ConversionType, Views.ConversionPage> conversionPageFactory)
        {
            _bd = bd;
            _navigation = navigation;
            _sp = sp;
            _conversionPageFactory = conversionPageFactory;

            NavigerConversionCommande = new Command<ConversionType>(
                async type => await NaviguerAsync(() => _conversionPageFactory(type)));
            NavigerHistoriqueCommande = new Command(
                async () => await NaviguerAsync(() => _sp.GetRequiredService<Views.HistoriquePage>()));
        }

        private async Task NaviguerAsync(Func<Page> fabrique)
        {
            try { await _navigation.PushAsync(fabrique()); }
            catch { /* navigation double-clic ignorée */ }
        }

        public void ChargerConversionsRecentes() =>
            ConversionsRecentes = _bd.ObtenirRecentes();

        private void OnPropertyChanged(string nomPropriete) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomPropriete));
    }
}
