using System;
using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models
{
    public class Employee
    {
        [JsonPropertyName("name")]
        public string Id { get; set; }
        
        [JsonPropertyName("last_name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("employee_name")]
        public string FullName { get; set; }

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("designation")]
        public string Position { get; set; }

        [JsonPropertyName("department")]
        public string Department { get; set; }

        [JsonPropertyName("date_of_joining")]
        public DateTime? HiringDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("company_email")]
        public string Email { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("image")]
        public string PhotoUrl { get; set; }
    }
}