using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ERPNextNewApp.Models.Dto
{
    public class SalaryStructureDto
    {
        [Required(ErrorMessage = "Le code de la structure est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le code de la structure ne peut pas être vide.")]
        public string StructureCode { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le nom ne peut pas être vide.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "L'abréviation est obligatoire.")]
        [MinLength(1, ErrorMessage = "L'abréviation ne peut pas être vide.")]
        public string Abbreviation { get; set; }

        [Required(ErrorMessage = "Le type est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le type ne peut pas être vide.")]
        public string Type { get; set; }

        [Required(ErrorMessage = "La formule est obligatoire.")]
        [MinLength(1, ErrorMessage = "La formule ne peut pas être vide.")]
        public string Formula { get; set; }

        [Required(ErrorMessage = "La société est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le nom de la société ne peut pas être vide.")]
        public string Company { get; set; }

        // ✅ Méthode pour vérifier la formule
        public bool EstFormuleValide()
        {
            if (string.IsNullOrWhiteSpace(Formula))
                return false;

            if (decimal.TryParse(Formula, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valeur))
            {
                return valeur >= 0;
            }

            // Si ce n'est pas un nombre, on considère que c'est valide
            return true;
        }
    }
}