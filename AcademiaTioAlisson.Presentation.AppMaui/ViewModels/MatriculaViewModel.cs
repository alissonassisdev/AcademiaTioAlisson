// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;
using AcademiaTioAlisson.Application.Enums;
using AcademiaTioAlisson.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LogradouroDto = AcademiaTioAlisson.Application.DTOs.LogradouroDto;

namespace AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    public IEnumerable<AppMatriculaPlano> PlanoTipos { get; } = Enum.GetValues<AppMatriculaPlano>().Cast<AppMatriculaPlano>();
    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = [];

    private bool _suspendSelectionSync = false;
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    private MatriculaDto _matricula = CriarMatriculaPadrao();
    public MatriculaDto Matricula
    {
        get => _matricula;
        set
        {
            if (SetProperty(ref _matricula, value))
            {
                SyncSelectionFromFlags();
            }
        }
    }

    private int _matriculaId;
    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public bool HasAlunoVinculado => Matricula?.AlunoMatricula != null && Matricula.AlunoMatricula.Id > 0;
    public bool HasNoAlunoVinculado => !HasAlunoVinculado;
    public bool IsMenor16 => Matricula?.AlunoMatricula != null && Matricula.AlunoMatricula.DataNascimento != default &&
                            Matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
    public bool HasRestricoes => Matricula != null && Matricula.RestricoesMedicas != AppMatriculaRestricoes.None;
    public bool ExigeLaudo => IsMenor16 || HasRestricoes;
    public bool HasLaudo => Matricula?.LaudoMedico?.Conteudo != null && Matricula.LaudoMedico.Conteudo.Length > 0;
    public bool LaudoPendente => ExigeLaudo && !HasLaudo;
    public bool LaudoOpcional => !ExigeLaudo && !HasLaudo;

    public void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
        OnPropertyChanged(nameof(IsMenor16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
        OnPropertyChanged(nameof(LaudoOpcional));
    }

    public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = "Detalhes da Matrícula";

        BuildRestricoesMulti();
    }

    public async Task InitializeAsync()
    {
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar Matrícula";
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova Matrícula";
            Matricula = CriarMatriculaPadrao();
            SyncSelectionFromFlags();
        }
        NotificarPropriedadesVisuais();
    }

    private static MatriculaDto CriarMatriculaPadrao() => new()
    {
        AlunoMatricula = new AlunoDto
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Email = string.Empty,
            Senha = string.Empty,
            Endereco = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            },
            Numero = string.Empty,
            Complemento = string.Empty
        },
        Plano = AppMatriculaPlano.Anual,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        DataFim = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
        Objetivo = string.Empty,
        RestricoesMedicas = AppMatriculaRestricoes.None,
        ObservacoesRestricoes = string.Empty,
        LaudoMedico = null
    };

    private void BuildRestricoesMulti()
    {
        RestricoesMulti.Clear();
        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>().Cast<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.None) continue;

            var item = new RestricaoSelectable
            {
                Value = value,
                IsSelected = Matricula.RestricoesMedicas.HasFlag(value)
            };
            item.PropertyChanged += RestricaoItem_PropertyChanged;
            RestricoesMulti.Add(item);
        }
    }

    private void RestricaoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RestricaoSelectable.IsSelected))
        {
            if (_suspendSelectionSync) return;
            RecomputeRestricoesFlags();
        }
    }

    private void RecomputeRestricoesFlags()
    {
        var flags = AppMatriculaRestricoes.None;
        foreach (var item in RestricoesMulti)
        {
            if (item.IsSelected)
                flags |= item.Value;
        }

        Matricula.RestricoesMedicas = flags;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    private void SyncSelectionFromFlags()
    {
        _suspendSelectionSync = true;
        try
        {
            foreach (var item in RestricoesMulti)
            {
                item.IsSelected = Matricula.RestricoesMedicas.HasFlag(item.Value);
            }
        }
        finally
        {
            _suspendSelectionSync = false;
        }
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    public void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    public async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var data = await _matriculaService.ObterPorIdAsync(MatriculaId, cts.Token);
            if (data != null)
            {
                Matricula = data;
                SyncSelectionFromFlags();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Matricula.AlunoMatricula?.Cpf))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var cpfLimpo = new string(Matricula.AlunoMatricula.Cpf.Where(char.IsDigit).ToArray());
            var alunoData = await _alunoService.ObterPorCpfAsync(cpfLimpo, cts.Token)
                         ?? await _alunoService.ObterPorCpfAsync(Matricula.AlunoMatricula.Cpf, cts.Token);

            if (alunoData != null)
            {
                Matricula.AlunoMatricula = alunoData;
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno encontrado! Dados preenchidos.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno não encontrado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem do Laudo Médico", "Cancelar", null, "Galeria", "Câmera", "Remover Laudo");
            if (escolha == "Remover Laudo")
            {
                RemoverLaudo();
                return;
            }

            FileResult? result = null;
            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o Laudo Médico",
                    FileTypes = FilePickerFileType.Images
                });
            }
            else if (escolha == "Câmera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Erro", "Captura de foto não suportada.", "OK");
                    return;
                }
            }

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Matricula.LaudoMedico = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task SaveMatriculaAsync()
    {
        if (IsBusy) return;

        RecomputeRestricoesFlags();

        if (!await ValidateMatriculaAsync(Matricula))
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            AlunoDto? alunoData = null;
            if (Matricula.AlunoMatricula?.Id > 0)
            {
                alunoData = await _alunoService.ObterPorIdAsync(Matricula.AlunoMatricula.Id, cts.Token);
            }
            else if (!string.IsNullOrWhiteSpace(Matricula.AlunoMatricula?.Cpf))
            {
                var cpfDigitos = new string(Matricula.AlunoMatricula.Cpf.Where(char.IsDigit).ToArray());
                alunoData = await _alunoService.ObterPorCpfAsync(cpfDigitos, cts.Token)
                         ?? await _alunoService.ObterPorCpfAsync(Matricula.AlunoMatricula.Cpf, cts.Token);
            }

            if (alunoData == null)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Aluno não encontrado. Busque um aluno pelo CPF.", "OK");
                return;
            }

            Matricula.AlunoMatricula = alunoData;

            if (!await ValidateMatriculaAsync(Matricula))
                return;

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula atualizada com sucesso!", "OK");
            }
            else
            {
                await _matriculaService.AdicionarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula criada com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<bool> ValidateMatriculaAsync(MatriculaDto matricula)
    {
        var errors = new List<string>();

        if (matricula.AlunoMatricula == null || (matricula.AlunoMatricula.Id <= 0 && string.IsNullOrWhiteSpace(matricula.AlunoMatricula.Cpf)))
            errors.Add("• Aluno é obrigatório. Informe o CPF e realize a busca.");

        if (matricula.DataInicio == default)
            errors.Add("• Data de início é obrigatória.");

        if (string.IsNullOrWhiteSpace(matricula.Objetivo))
            errors.Add("• Objetivo da matrícula é obrigatório.");

        if (!Enum.IsDefined(matricula.Plano))
            errors.Add("• Plano da matrícula é inválido.");

        bool menor16 = matricula.AlunoMatricula != null && matricula.AlunoMatricula.DataNascimento != default &&
                       matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        bool possuiLaudo = matricula.LaudoMedico?.Conteudo != null && matricula.LaudoMedico.Conteudo.Length > 0;

        if (menor16 && !possuiLaudo)
            errors.Add("• Alunos menores de 16 anos devem obrigatoriamente apresentar laudo médico.");

        if (matricula.RestricoesMedicas != AppMatriculaRestricoes.None && !possuiLaudo)
            errors.Add("• Alunos com restrições médicas cadastradas devem obrigatoriamente apresentar laudo médico.");

        if (errors.Count > 0)
        {
            var msg = "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", errors);
            await Shell.Current.DisplayAlertAsync("Erros de Validação", msg, "OK");
            return false;
        }

        return true;
    }
}