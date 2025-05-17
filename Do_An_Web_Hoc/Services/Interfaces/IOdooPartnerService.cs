using Do_An_Web_Hoc.Models.Odoo;
using System.Threading.Tasks;

namespace Do_An_Web_Hoc.Services.Interfaces
{
    public interface IOdooPartnerService
    {
        Task<int?> SearchPartnerByEmailAsync(string email);
        Task<int?> CreatePartnerAsync(OdooPartnerDto dto);
        Task<bool> UpdatePartnerAsync(int partnerId, OdooPartnerDto dto);
        Task<int?> SearchRoleIdByNameAsync(string roleName);

    }

}
