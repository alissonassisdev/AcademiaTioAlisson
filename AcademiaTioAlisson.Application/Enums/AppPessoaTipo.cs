// Alisson Assis
using System.ComponentModel.DataAnnotations;

namespace AcademiaTioAlisson.Application.Enums;

public enum AppPessoaTipo
{
    [Display(Name = "Aluno")]
    Aluno = 1,

    [Display(Name = "Colaborador")]
    Colaborador = 2
}