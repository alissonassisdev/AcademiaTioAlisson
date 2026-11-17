// Alisson Assis
using AcademiaTioAlisson.Application.DTOs;

namespace AcademiaTioAlisson.Application.Interfaces;

public interface IAcessoService
{
    Task<AcessoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<AcessoDto> AdicionarAsync(AcessoDto acessoDto, CancellationToken cancellationToken = default);
    Task<AcessoDto> AtualizarAsync(AcessoDto acessoDto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}