// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AlunoListViewModel vm)
            await vm.LoadAlunosCommand.ExecuteAsync(null);
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is AlunoDto dto && BindingContext is AlunoListViewModel vm)
            await vm.EditAlunoCommand.ExecuteAsync(dto);
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is AlunoDto dto && BindingContext is AlunoListViewModel vm)
            await vm.DeleteAlunoCommand.ExecuteAsync(dto);
    }
}