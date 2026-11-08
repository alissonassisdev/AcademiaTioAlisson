// Alisson Assis
using AcademiaTioAlisson.Application.Enums;
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

namespace AcademiaTioAlisson.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaViewModel vm)
        {
            await vm.InitializeAsync();
            CalculateFimFromPlanoAndInicio();
        }
    }

    private void PlanoPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }

    private void InicioDatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }

    private void CalculateFimFromPlanoAndInicio()
    {
        if (BindingContext is not MatriculaViewModel vm) return;

        var plano = vm.Matricula.Plano;
        var inicio = vm.Matricula.DataInicio;

        DateOnly fim = plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };

        vm.Matricula.DataFim = fim;

        try
        {
            FimDatePicker.Date = fim.ToDateTime(new TimeOnly(0, 0));
        }
        catch { }
    }
}