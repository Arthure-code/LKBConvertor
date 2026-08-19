using LKBConvertor.Models;
using LKBConvertor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LKBConvertor.Views;

public partial class ConversionPage : ContentPage
{
    public ConversionPage(ConversionType type, IServiceProvider sp)
    {
        InitializeComponent();

        BindingContext = ActivatorUtilities.CreateInstance<ConversionViewModel>(sp, type);

        Title = type switch
        {
            ConversionType.WordVersPdf       => "Word → PDF",
            ConversionType.PdfVersRtf        => "PDF → RTF",
            ConversionType.PdfVersWord       => "PDF → Word",
            ConversionType.ImageVersPdf      => "Image → PDF",
            ConversionType.ImageVersWord     => "Image → Word",
            ConversionType.ExcelVersPdf      => "Excel → PDF",
            ConversionType.PowerPointVersPdf => "PowerPoint → PDF",
            ConversionType.PdfVersImage      => "PDF → Image",
            _ => "Conversion"
        };
    }
}
