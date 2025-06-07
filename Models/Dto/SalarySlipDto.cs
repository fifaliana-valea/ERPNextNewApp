using System;
using System.ComponentModel.DataAnnotations;

namespace ERPNextNewApp.Models.Dto
{
    public class SalarySlipDto
    {
        [Required(ErrorMessage = "La date de début est obligatoire.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "L'identifiant de l'employé est obligatoire.")]
        [MinLength(1, ErrorMessage = "L'identifiant de l'employé ne peut pas être vide.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Le montant de base est obligatoire.")]
        [Range(0, double.MaxValue, ErrorMessage = "Le montant de base ne peut pas être négatif.")]
        public decimal BaseAmount { get; set; }

        [Required(ErrorMessage = "Le code de la structure est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le code de la structure ne peut pas être vide.")]
        public string StructureCode { get; set; }
    }
}