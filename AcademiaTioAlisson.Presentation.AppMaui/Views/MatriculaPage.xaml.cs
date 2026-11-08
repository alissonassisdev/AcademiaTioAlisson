// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class MatriculaListPage : ContentPage
{
    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaListViewModel vm)
            await vm.LoadMatriculasCommand.ExecuteAsync(null);
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is MatriculaDto dto && BindingContext is MatriculaListViewModel vm)
            await vm.EditMatriculaCommand.ExecuteAsync(dto);
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is MatriculaDto dto && BindingContext is MatriculaListViewModel vm)
            await vm.DeleteMatriculaCommand.ExecuteAsync(dto);
    }
}