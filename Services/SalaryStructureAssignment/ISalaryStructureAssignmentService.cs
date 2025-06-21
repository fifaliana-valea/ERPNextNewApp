namespace ERPNextNewApp.Services.SalaryStructureAssignment;

public interface ISalaryStructureAssignmentService
{
    Task<List<Models.Salary.SalaryStructureAssignment>> GetSalaryAssigmentAsync(string? employeId = null, DateTime? dateDebut = null,DateTime? dateFin = null);

    Task<Models.Salary.SalaryStructureAssignment?> GetSalaryAssigmentByIdAsync(string salaryAssigmentId);
    Task<bool> InsertSalaryStructureAssigmentAsync(Models.Salary.SalaryStructureAssignment salaryStructureAssignment);

    Task<bool> UpdateSalaryStructureAssigmentAsync(Models.Salary.SalaryStructureAssignment salaryStructureAssignment);

    Task<bool> DeleteAssigmentsAsync(string assigment);

    Task<bool> ModificationAssigmentsAsync(Models.Salary.SalaryStructureAssignment assignment);

    Task<(bool resultat, string message)> ModificationWithCondition(decimal salary, int condition, string componentName,
        int pourcentage, int action);
    Task<(bool resultat,string message)> InsertWithConditionAsync(List<DateTime> dateTimes, string employee, decimal salary);
}