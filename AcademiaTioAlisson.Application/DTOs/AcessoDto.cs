// Alisson Assis
using AcademiaTioAlisson.Application.Enums;

namespace AcademiaTioAlisson.Application.DTOs;

public class AcessoDto
{
    public int Id { get; set; }
    public AppPessoaTipo Tipo { get; set; }
    public AlunoDto? AlunoAcesso { get; set; }
    public ColaboradorDto? ColaboradorAcesso { get; set; }
    public DateTime DataHoraEntrada { get; set; }
    public DateTime? DataHoraSaida { get; set; }
}