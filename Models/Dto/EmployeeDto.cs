using System;
using System.ComponentModel.DataAnnotations;

namespace ERPNextNewApp.Models.Dto
{
    public class EmployeeDto
    {
        [Required(ErrorMessage = "L'identifiant est obligatoire.")]
        [MinLength(1, ErrorMessage = "L'identifiant ne peut pas être vide.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le nom ne peut pas être vide.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le prénom ne peut pas être vide.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le genre est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le genre ne peut pas être vide.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "La date d'embauche est obligatoire.")]
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "La date de naissance est obligatoire.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Le nom de la société est obligatoire.")]
        [MinLength(1, ErrorMessage = "Le nom de la société ne peut pas être vide.")]
        public string Company { get; set; }
    }
}