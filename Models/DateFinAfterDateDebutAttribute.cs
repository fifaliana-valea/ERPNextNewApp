using System.ComponentModel.DataAnnotations;

namespace ERPNextNewApp.Models;

public class DateFinAfterDateDebutAttribute : ValidationAttribute
{
    private readonly string _dateDebutPropertyName;

    public DateFinAfterDateDebutAttribute(string dateDebutPropertyName)
    {
        _dateDebutPropertyName = dateDebutPropertyName;
        ErrorMessage = "La date de fin doit être supérieure ou égale à la date de début.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var dateFin = value as DateTime?;
        var dateDebutProperty = validationContext.ObjectType.GetProperty(_dateDebutPropertyName);

        if (dateDebutProperty == null)
            return new ValidationResult($"Propriété inconnue: {_dateDebutPropertyName}");

        var dateDebut = dateDebutProperty.GetValue(validationContext.ObjectInstance) as DateTime?;

        if (dateDebut.HasValue && dateFin.HasValue && dateFin < dateDebut)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}