// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;
using AcademiaTioAlisson.Application.Interfaces;
using System.Text.Json;

namespace AcademiaTioAlisson.Application.Services;

public class AcessoService : IAcessoService
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "acessos_academiatioalisson.json");

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private static async Task<List<AcessoDto>> LerArquivoAsync()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<AcessoDto>>(json, _jsonOptions) ?? [];
    }

    private static async Task SalvarArquivoAsync(List<AcessoDto> lista)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(lista, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task<AcessoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var lista = await LerArquivoAsync();
            return lista.FirstOrDefault(a => a.Id == id);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<AcessoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await LerArquivoAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<AcessoDto> AdicionarAsync(AcessoDto acessoDto, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var lista = await LerArquivoAsync();
            acessoDto.Id = lista.Count > 0 ? lista.Max(a => a.Id) + 1 : 1;
            lista.Add(acessoDto);
            await SalvarArquivoAsync(lista);
            return acessoDto;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<AcessoDto> AtualizarAsync(AcessoDto acessoDto, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var lista = await LerArquivoAsync();
            var index = lista.FindIndex(a => a.Id == acessoDto.Id);
            if (index >= 0)
            {
                lista[index] = acessoDto;
                await SalvarArquivoAsync(lista);
            }
            return acessoDto;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var lista = await LerArquivoAsync();
            var removido = lista.RemoveAll(a => a.Id == id) > 0;
            if (removido)
                await SalvarArquivoAsync(lista);
            return removido;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}