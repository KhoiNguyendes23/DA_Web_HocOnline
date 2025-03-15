using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IUserAccountRepository
    {
        // Tìm người dùng bằng email.
        Task<UserAccount> GetByEmailAsync(string email);
        // Lấy danh sách người dùng theo vai trò (RoleID)
        Task<IEnumerable<UserAccount>> GetUsersByRoleAsync(int roleId);
        // Kiểm tra người dùng đã tồn tại chưa.
        Task<bool> CheckUserExistsAsync(string email);
        // Cập nhật trạng thái người dùng
        Task UpdateUserStatusAsync(int userId, int status);

        // Đăng ký người dùng mới
        Task<UserAccount> RegisterAsync(UserAccount user, string password);

        // Đăng nhập người dùng
        Task<UserAccount> LoginAsync(string email, string password);
    }
}
