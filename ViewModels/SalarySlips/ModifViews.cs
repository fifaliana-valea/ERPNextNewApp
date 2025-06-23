using System.ComponentModel.DataAnnotations;
using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.ViewModels.SalarySlips
{
    public class ModifViews : IValidatableObject
    {
        public string? Component { get; set; }

        public int Condition { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le salaire doit être supérieur ou égal à 0.")]
        public decimal Salary { get; set; }

        [Range(0, 100, ErrorMessage = "Le pourcentage doit être compris entre 0 et 100.")]
        public int Pourcentage { get; set; }

        public int Action { get; set; }

        public List<SalaryComponent>? SalaireSalaire { get; set; }

        public List<SalarySlip>? SalarySlips { get; set; }

        // ✅ Validation personnalisée ici
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Salary < 0)
            {
                yield return new ValidationResult("Le salaire ne peut pas être négatif.", new[] { nameof(Salary) });
            }

            if (Pourcentage < 0 || Pourcentage > 100)
            {
                yield return new ValidationResult("Le pourcentage doit être entre 0 et 100.", new[] { nameof(Pourcentage) });
            }

            // Tu peux ajouter d'autres règles ici
        }
    }
}