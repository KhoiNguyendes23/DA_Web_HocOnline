using Do_An_Web_Hoc.Models;


namespace Do_An_Web_Hoc.Services.Interfaces
{
    public interface IOdooEnrollmentService
    {
        Task<int?> CreateEnrollmentAsync(int studentOdooId, int courseOdooId);

        Task<int?> SearchEnrollmentAsync(int studentOdooId, int courseOdooId);

        Task<bool> UpdateEnrollmentAsync(int enrollmentId, Dictionary<string, object> fieldsToUpdate);

        Task<int> SyncAllEnrollmentsToOdooAsync(IEnumerable<Enrollments> enrollments);
    }
}
