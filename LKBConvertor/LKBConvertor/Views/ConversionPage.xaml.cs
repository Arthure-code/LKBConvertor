using LKBConvertor.Models;
using LKBConvertor.ViewModels;

namespace LKBConvertor.Views;

public partial class ConversionPage : ContentPage
{
    public ConversionPage(ConversionType type)
    {
        InitializeComponent();

        var vm = new ConversionViewModel(type);
        Resources["vm"] = vm;

        Title = type == ConversionType.WordVersPdf
            ? "Word → PDF"
            : "PDF → RTF";
    }
}