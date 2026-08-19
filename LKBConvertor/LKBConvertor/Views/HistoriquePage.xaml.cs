using LKBConvertor.ViewModels;

namespace LKBConvertor.Views;

public partial class HistoriquePage : ContentPage
{
    public HistoriquePage(HistoriqueViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as HistoriqueViewModel)?.ChargerHistorique();
    }
}
