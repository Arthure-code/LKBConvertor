using LKBConvertor.ViewModels;

namespace LKBConvertor.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = Resources["vm"] as HomeViewModel;
        vm?.ChargerConversionsRecentes();
    }
}