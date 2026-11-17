// Alisson Assis
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AcessoViewModel vm)
            await vm.InitializeAsync();
    }
}