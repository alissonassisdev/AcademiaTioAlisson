// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorListViewModel vm)
            await vm.LoadColaboradoresCommand.ExecuteAsync(null);
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is ColaboradorDto dto && BindingContext is ColaboradorListViewModel vm)
            await vm.EditColaboradorCommand.ExecuteAsync(dto);
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is ColaboradorDto dto && BindingContext is ColaboradorListViewModel vm)
            await vm.DeleteColaboradorCommand.ExecuteAsync(dto);
    }
}