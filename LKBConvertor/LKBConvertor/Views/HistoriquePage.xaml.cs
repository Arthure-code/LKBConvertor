using LKBConvertor.ViewModels;

namespace LKBConvertor.Views;

public partial class HistoriquePage : ContentPage
{
    public HistoriquePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = Resources["vm"] as HistoriqueViewModel;
        vm?.ChargerHistorique();
    }
}