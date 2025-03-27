using Do_An_Web_Hoc.Models;

namespace Do_An_Web_Hoc.Repositories.Interfaces
{
    public interface IUserAccountRepository
    {
        // Tìm người dùng bằng email.
        Task<UserAccount> GetByEmailAsync(string email);
        // Lấy danh sách người dùng theo vai trò (RoleID)
        Task<IEnumerable<UserAccount>> GetUsersByRoleAsync(int roleId);
        // Lấy toàn bộ người dùng
        Task<IEnumerable<UserAccount>> GetAllUsersAsync();
        // Lưu toàn bộ danh sách người dùng sau khi cập nhật
        Task SaveAllUsersAsync(IEnumerable<UserAccount> users);
        // Kiểm tra người dùng đã tồn tại chưa.
        Task<bool> CheckUserExistsAsync(string email);
        // Cập nhật trạng thái người dùng
        Task UpdateUserStatusAsync(int userId, int status);
        //Sửa người dùng
        Task UpdateUserAsync(UserAccount user);
       Task UpdateAsync (UserAccount user);
        // Đăng ký người dùng mới
        Task<UserAccount> RegisterAsync(UserAccount user, string password);

        //Tự Cập nhật thông tin người dùng 
        Task UpdateUserInfoAsync(UserAccount updatedUser);

        // Đăng nhập người dùng
        Task<UserAccount> LoginAsync(string email, string password);
        // Thêm các phương thức sau cho OTP
        Task<bool> SendOTPAsync(string email);
        Task<bool> VerifyOTPAsync(string email, string otp);
        Task<bool> ResetPasswordByOTPAsync(string email, string newPassword);
    }
}
