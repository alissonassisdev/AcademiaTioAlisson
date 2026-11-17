// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;
using AcademiaTioAlisson.Application.Enums;
using AcademiaTioAlisson.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaTioAlisson.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAcessoService _acessoService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    private List<AlunoDto> _todosAlunos = [];
    private List<ColaboradorDto> _todosColaboradores = [];

    // Alternância de Modo (Aluno ou Colaborador)
    private bool _isModoAluno = true;
    public bool IsModoAluno
    {
        get => _isModoAluno;
        set
        {
            if (SetProperty(ref _isModoAluno, value))
            {
                OnPropertyChanged(nameof(IsModoColaborador));
                OnPropertyChanged(nameof(ModoSelecionadoTexto));
                OnPropertyChanged(nameof(LabelSelecaoTexto));
                AtualizarListaFiltrada();
            }
        }
    }

    public bool IsModoColaborador => !IsModoAluno;
    public string ModoSelecionadoTexto => IsModoAluno ? "Modo atual selecionado: Aluno" : "Modo atual selecionado: Colaborador";
    public string LabelSelecaoTexto => IsModoAluno ? "Selecione o Aluno:" : "Selecione o Colaborador:";

    // Filtro e Lista do Picker
    private string _filtroTexto = string.Empty;
    public string FiltroTexto
    {
        get => _filtroTexto;
        set => SetProperty(ref _filtroTexto, value);
    }

    public ObservableCollection<PessoaAcessoItem> PessoasFiltradas { get; } = [];

    private PessoaAcessoItem? _selectedPessoa;
    public PessoaAcessoItem? SelectedPessoa
    {
        get => _selectedPessoa;
        set
        {
            if (SetProperty(ref _selectedPessoa, value))
            {
                _ = AtualizarStatusPessoaSelecionadaAsync();
            }
        }
    }

    public bool HasPessoaSelecionada => SelectedPessoa != null;

    // Estado na Academia (Dentro ou Fora)
    private bool _isDentroDaAcademia;
    public bool IsDentroDaAcademia
    {
        get => _isDentroDaAcademia;
        set
        {
            if (SetProperty(ref _isDentroDaAcademia, value))
            {
                OnPropertyChanged(nameof(StatusAcademiaTexto));
                OnPropertyChanged(nameof(BotaoAcaoTexto));
            }
        }
    }

    public string StatusAcademiaTexto => IsDentroDaAcademia ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";
    public string BotaoAcaoTexto => IsDentroDaAcademia ? "Registrar Saída" : "Registrar Entrada";

    // Banner de Feedback (Acesso Negado / Entrada Autorizada / Saída Registrada)
    private bool _hasFeedback;
    public bool HasFeedback
    {
        get => _hasFeedback;
        set => SetProperty(ref _hasFeedback, value);
    }

    private bool _isFeedbackErro;
    public bool IsFeedbackErro
    {
        get => _isFeedbackErro;
        set => SetProperty(ref _isFeedbackErro, value);
    }

    private string _feedbackTitulo = string.Empty;
    public string FeedbackTitulo
    {
        get => _feedbackTitulo;
        set => SetProperty(ref _feedbackTitulo, value);
    }

    private string _feedbackMensagem = string.Empty;
    public string FeedbackMensagem
    {
        get => _feedbackMensagem;
        set => SetProperty(ref _feedbackMensagem, value);
    }

    // Lista de Acessos Registrados Hoje
    public ObservableCollection<AcessoRegistroItem> AcessosHoje { get; } = [];

    public AcessoViewModel(
        IAcessoService acessoService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IMatriculaService matriculaService)
    {
        _acessoService = acessoService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        Title = "Controle de Acesso";
    }

    public async Task InitializeAsync()
    {
        await CarregarDadosIniciaisAsync();
    }

    [RelayCommand]
    private void SelecionarAluno()
    {
        IsModoAluno = true;
        HasFeedback = false;
    }

    [RelayCommand]
    private void SelecionarColaborador()
    {
        IsModoAluno = false;
        HasFeedback = false;
    }

    [RelayCommand]
    private void FiltrarPessoas()
    {
        AtualizarListaFiltrada();
    }

    [RelayCommand]
    public async Task CarregarDadosIniciaisAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            _todosAlunos = [.. (await _alunoService.ObterTodosAsync(cts.Token) ?? [])];
            _todosColaboradores = [.. (await _colaboradorService.ObterTodosAsync(cts.Token) ?? [])];

            AtualizarListaFiltrada();
            await AtualizarHistoricoHojeAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AtualizarListaFiltrada()
    {
        PessoasFiltradas.Clear();
        var termo = FiltroTexto?.Trim() ?? string.Empty;
        var termoDigitos = new string(termo.Where(char.IsDigit).ToArray());

        if (IsModoAluno)
        {
            var filtrados = string.IsNullOrWhiteSpace(termo)
                ? _todosAlunos
                : _todosAlunos.Where(a =>
                    a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(termoDigitos) && a.Cpf.Contains(termoDigitos)));

            foreach (var a in filtrados)
            {
                PessoasFiltradas.Add(new PessoaAcessoItem
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Cpf = a.Cpf,
                    Foto = a.Foto,
                    IsAluno = true,
                    Aluno = a
                });
            }
        }
        else
        {
            var filtrados = string.IsNullOrWhiteSpace(termo)
                ? _todosColaboradores
                : _todosColaboradores.Where(c =>
                    c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(termoDigitos) && c.Cpf.Contains(termoDigitos)));

            foreach (var c in filtrados)
            {
                PessoasFiltradas.Add(new PessoaAcessoItem
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Cpf = c.Cpf,
                    Foto = c.Foto,
                    IsAluno = false,
                    Colaborador = c
                });
            }
        }

        SelectedPessoa = PessoasFiltradas.FirstOrDefault();
        OnPropertyChanged(nameof(HasPessoaSelecionada));
    }

    private async Task AtualizarStatusPessoaSelecionadaAsync()
    {
        OnPropertyChanged(nameof(HasPessoaSelecionada));
        if (SelectedPessoa == null)
        {
            IsDentroDaAcademia = false;
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var todosAcessos = await _acessoService.ObterTodosAsync(cts.Token) ?? [];
            var hoje = DateTime.Today;

            var registroAberto = todosAcessos.FirstOrDefault(a =>
                a.DataHoraEntrada.Date == hoje &&
                a.DataHoraSaida == null &&
                ((SelectedPessoa.IsAluno && a.AlunoAcesso?.Id == SelectedPessoa.Id) ||
                 (!SelectedPessoa.IsAluno && a.ColaboradorAcesso?.Id == SelectedPessoa.Id)));

            IsDentroDaAcademia = registroAberto != null;
        }
        catch
        {
            IsDentroDaAcademia = false;
        }
    }

    [RelayCommand]
    public async Task RegistrarAcessoAsync()
    {
        if (IsBusy || SelectedPessoa == null) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var agora = DateTime.Now;
            var hoje = DateOnly.FromDateTime(agora);

            var todosAcessos = (await _acessoService.ObterTodosAsync(cts.Token) ?? []).ToList();

            // Verifica se já existe entrada aberta hoje sem saída
            var acessoAberto = todosAcessos.FirstOrDefault(a =>
                a.DataHoraEntrada.Date == agora.Date &&
                a.DataHoraSaida == null &&
                ((SelectedPessoa.IsAluno && a.AlunoAcesso?.Id == SelectedPessoa.Id) ||
                 (!SelectedPessoa.IsAluno && a.ColaboradorAcesso?.Id == SelectedPessoa.Id)));

            if (SelectedPessoa.IsAluno && SelectedPessoa.Aluno != null)
            {
                if (acessoAberto == null)
                {
                    // REGRA ALUNO - ENTRADA: Validar matrícula ativa
                    var matriculasAtivas = await _matriculaService.ObterAtivasAsync(SelectedPessoa.Id, cts.Token) ?? [];
                    var matriculaValida = matriculasAtivas
                        .Where(m => m.DataFim >= hoje)
                        .OrderByDescending(m => m.DataFim)
                        .FirstOrDefault();

                    if (matriculaValida == null)
                    {
                        ExibirFeedback(erro: true, "Acesso Negado", "Acesso negado: o aluno não possui matrícula ativa.");
                        return;
                    }

                    int diasRestantes = (matriculaValida.DataFim.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;

                    var novoAcesso = new AcessoDto
                    {
                        Tipo = AppPessoaTipo.Aluno,
                        AlunoAcesso = SelectedPessoa.Aluno,
                        DataHoraEntrada = agora,
                        DataHoraSaida = null
                    };

                    await _acessoService.AdicionarAsync(novoAcesso, cts.Token);
                    IsDentroDaAcademia = true;

                    string msgEntrada = $"Entrada autorizada. Dias restantes de plano: {diasRestantes} dia(s).";
                    ExibirFeedback(erro: false, "Entrada Autorizada", msgEntrada);
                    AdicionarLogTela(SelectedPessoa.Nome, agora, msgEntrada, isEntrada: true);
                }
                else
                {
                    // REGRA ALUNO - SAÍDA: Mostrar tempo de permanência
                    acessoAberto.DataHoraSaida = agora;
                    await _acessoService.AtualizarAsync(acessoAberto, cts.Token);
                    IsDentroDaAcademia = false;

                    TimeSpan permanencia = agora - acessoAberto.DataHoraEntrada;
                    string msgSaida = $"Saída registrada. Tempo de permanência: {permanencia:hh\\:mm\\:ss}.";
                    ExibirFeedback(erro: false, "Saída Registrada", msgSaida);
                    AdicionarLogTela(SelectedPessoa.Nome, agora, msgSaida, isEntrada: false);
                }
            }
            else if (!SelectedPessoa.IsAluno && SelectedPessoa.Colaborador != null)
            {
                // Soma todas as horas trabalhadas hoje pelo colaborador
                var registrosHojeColab = todosAcessos
                    .Where(a => a.DataHoraEntrada.Date == agora.Date && a.ColaboradorAcesso?.Id == SelectedPessoa.Id)
                    .ToList();

                TimeSpan totalTrabalhadoHoje = TimeSpan.Zero;
                foreach (var reg in registrosHojeColab)
                {
                    if (reg.DataHoraSaida.HasValue)
                        totalTrabalhadoHoje += reg.DataHoraSaida.Value - reg.DataHoraEntrada;
                }

                if (acessoAberto == null)
                {
                    // REGRA COLABORADOR - ENTRADA: Validar limite de 8h (CLT) ou 6h (Estágio)
                    int limiteHoras = SelectedPessoa.Colaborador.Vinculo == AppColaboradorVinculo.Estagio ? 6 : 8;
                    if (totalTrabalhadoHoje >= TimeSpan.FromHours(limiteHoras))
                    {
                        ExibirFeedback(erro: true, "Acesso Negado", $"Acesso negado: limite diário de {limiteHoras}h ({SelectedPessoa.Colaborador.Vinculo}) já atingido.");
                        return;
                    }

                    var novoAcesso = new AcessoDto
                    {
                        Tipo = AppPessoaTipo.Colaborador,
                        ColaboradorAcesso = SelectedPessoa.Colaborador,
                        DataHoraEntrada = agora,
                        DataHoraSaida = null
                    };

                    await _acessoService.AdicionarAsync(novoAcesso, cts.Token);
                    IsDentroDaAcademia = true;

                    string msgEntrada = $"Entrada registrada. Horas trabalhadas hoje até o momento: {totalTrabalhadoHoje:hh\\:mm\\:ss}.";
                    ExibirFeedback(erro: false, "Entrada Autorizada", msgEntrada);
                    AdicionarLogTela(SelectedPessoa.Nome, agora, msgEntrada, isEntrada: true);
                }
                else
                {
                    // REGRA COLABORADOR - SAÍDA: Mostrar tempo da sessão + soma total do dia
                    acessoAberto.DataHoraSaida = agora;
                    await _acessoService.AtualizarAsync(acessoAberto, cts.Token);
                    IsDentroDaAcademia = false;

                    TimeSpan permanenciaSessao = agora - acessoAberto.DataHoraEntrada;
                    TimeSpan totalDiaAtualizado = totalTrabalhadoHoje + permanenciaSessao;

                    string msgSaida = $"Saída registrada. Tempo de permanência: {permanenciaSessao:hh\\:mm\\:ss} (Total hoje: {totalDiaAtualizado:hh\\:mm\\:ss}).";
                    ExibirFeedback(erro: false, "Saída Registrada", msgSaida);
                    AdicionarLogTela(SelectedPessoa.Nome, agora, msgSaida, isEntrada: false);
                }
            }
        }
        catch (Exception ex)
        {
            ExibirFeedback(erro: true, "Erro no Registro", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AtualizarHistoricoHojeAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var todos = await _acessoService.ObterTodosAsync(cts.Token) ?? [];
            var hoje = DateTime.Today;

            var doDia = todos
                .Where(a => a.DataHoraEntrada.Date == hoje)
                .OrderByDescending(a => a.DataHoraSaida ?? a.DataHoraEntrada)
                .ToList();

            AcessosHoje.Clear();
            foreach (var item in doDia)
            {
                string nome = item.AlunoAcesso?.Nome ?? item.ColaboradorAcesso?.Nome ?? $"Registro #{item.Id}";
                if (item.DataHoraSaida.HasValue)
                {
                    var duracao = item.DataHoraSaida.Value - item.DataHoraEntrada;
                    AcessosHoje.Add(new AcessoRegistroItem
                    {
                        NomePessoa = nome,
                        Horario = item.DataHoraSaida.Value.ToString("HH:mm:ss"),
                        Mensagem = $"Saída registrada. Tempo de permanência: {duracao:hh\\:mm\\:ss}.",
                        IsEntrada = false
                    });
                }
                else
                {
                    AcessosHoje.Add(new AcessoRegistroItem
                    {
                        NomePessoa = nome,
                        Horario = item.DataHoraEntrada.ToString("HH:mm:ss"),
                        Mensagem = "Entrada registrada na academia.",
                        IsEntrada = true
                    });
                }
            }
        }
        catch { }
    }

    private void ExibirFeedback(bool erro, string titulo, string mensagem)
    {
        IsFeedbackErro = erro;
        FeedbackTitulo = titulo;
        FeedbackMensagem = mensagem;
        HasFeedback = true;
    }

    private void AdicionarLogTela(string nome, DateTime horario, string mensagem, bool isEntrada)
    {
        AcessosHoje.Insert(0, new AcessoRegistroItem
        {
            NomePessoa = nome,
            Horario = horario.ToString("HH:mm:ss"),
            Mensagem = mensagem,
            IsEntrada = isEntrada
        });
    }
}

public class PessoaAcessoItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public ArquivoDto? Foto { get; set; }
    public bool IsAluno { get; set; }
    public AlunoDto? Aluno { get; set; }
    public ColaboradorDto? Colaborador { get; set; }

    public override string ToString() => $"{Nome} ({Cpf})";
}