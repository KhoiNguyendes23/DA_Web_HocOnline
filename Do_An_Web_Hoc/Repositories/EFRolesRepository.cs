using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFRolesRepository : IRolesRepository
    {
        private readonly ApplicationDbContext _context;

        public EFRolesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Roles>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }

}
