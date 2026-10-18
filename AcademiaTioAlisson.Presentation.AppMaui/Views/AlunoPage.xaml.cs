// Alisson Assis
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class AlunoPage : ContentPage
{
    public AlunoPage(AlunoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AlunoViewModel vm)
            await vm.InitializeAsync();
    }
}