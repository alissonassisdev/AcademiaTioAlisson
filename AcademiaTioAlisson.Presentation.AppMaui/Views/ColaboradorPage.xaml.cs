// Alisson Assis
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage
{
    public ColaboradorPage(ColaboradorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorViewModel vm)
            await vm.InitializeAsync();
    }
}