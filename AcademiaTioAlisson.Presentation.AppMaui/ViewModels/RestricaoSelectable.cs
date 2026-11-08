// Alisson Assis
using AcademiaTioAlisson.Application.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

public class RestricaoSelectable : ObservableObject
{
    public AppMatriculaRestricoes Value { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}