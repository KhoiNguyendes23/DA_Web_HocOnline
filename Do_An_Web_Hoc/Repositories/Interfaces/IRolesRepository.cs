using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        Task<IEnumerable<Roles>> GetAllRolesAsync();
    }
}
