// Alisson Assis
using AcademiaTioAlisson.Presentation.AppMaui.Configuration;
using AcademiaTioAlisson.Presentation.AppMaui.ViewModels;
using AcademiaTioAlisson.Presentation.AppMaui.Views;
using AcademiaTioAlisson.Application.Interfaces;
using AcademiaTioAlisson.Application.Services;
using Microsoft.Extensions.Logging;

namespace AcademiaTioAlisson.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        ConfigurationHelper.ConfigureServices(builder.Services);

        builder.Services.AddTransient<IAcessoService, AcessoService>();
        // ViewModels
        builder.Services.AddTransient<DashboardListViewModel>();
        builder.Services.AddTransient<LogradouroListViewModel>();
        builder.Services.AddTransient<LogradouroViewModel>();

        // Views
        builder.Services.AddTransient<DashboardListPage>();
        builder.Services.AddTransient<LogradouroListPage>();
        builder.Services.AddTransient<LogradouroPage>();
        builder.Services.AddTransient<ConfigPage>();


        // ViewModels
        builder.Services.AddTransient<ColaboradorListViewModel>();
        builder.Services.AddTransient<ColaboradorViewModel>();
        builder.Services.AddTransient<AlunoListViewModel>();
        builder.Services.AddTransient<AlunoViewModel>();

        // Views
        builder.Services.AddTransient<ColaboradorListPage>();
        builder.Services.AddTransient<ColaboradorPage>();
        builder.Services.AddTransient<AlunoListPage>();
        builder.Services.AddTransient<AlunoPage>();

        // ViewModels
        builder.Services.AddTransient<MatriculaListViewModel>();
        builder.Services.AddTransient<MatriculaViewModel>();

        // Views
        builder.Services.AddTransient<MatriculaListPage>();
        builder.Services.AddTransient<MatriculaPage>();

        // ViewModels
        builder.Services.AddTransient<AcessoViewModel>();

        // Views
        builder.Services.AddTransient<AcessoPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}