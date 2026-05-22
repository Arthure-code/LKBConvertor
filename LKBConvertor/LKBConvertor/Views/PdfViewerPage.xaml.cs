using Syncfusion.Maui.PdfViewer;

namespace LKBConvertor.Views;

public partial class PdfViewerPage : ContentPage
{
    private string _cheminPdf;

    public PdfViewerPage(string cheminPdf)
    {
        InitializeComponent();
        _cheminPdf = cheminPdf;
        Title = Path.GetFileName(cheminPdf);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        pdfViewer.DocumentSource = File.OpenRead(_cheminPdf);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        pdfViewer.DocumentSource = null;
    }
}