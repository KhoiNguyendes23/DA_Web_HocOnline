using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Do_An_Web_Hoc.Repositories
{
    public class EFUserAccountRepository :  IUserAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public EFUserAccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tìm người dùng theo email.
        public async Task<UserAccount> GetByEmailAsync(string email)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Lấy danh sách người dùng theo RoleID (vai trò).
        public async Task<IEnumerable<UserAccount>> GetUsersByRoleAsync(int roleId)
        {
            return await _context.UserAccounts
                .Where(u => u.RoleID == roleId)
                .ToListAsync();
        }

        // Kiểm tra người dùng tồn tại hay không dựa trên email.
        public async Task<bool> CheckUserExistsAsync(string email)
        {
            return await _context.UserAccounts
                .AnyAsync(u => u.Email == email);
        }

        // Cập nhật trạng thái của người dùng.
        public async Task UpdateUserStatusAsync(int userId, int status)
        {
            var user = await _context.UserAccounts.FindAsync(userId);
            if (user != null)
            {
                user.Status = status;
                await _context.SaveChangesAsync();
            }
        }
        // Đăng ký người dùng mới (KHÔNG MÃ HÓA MẬT KHẨU)
        public async Task<UserAccount> RegisterAsync(UserAccount user, string password)
        {
            // Kiểm tra email đã tồn tại chưa
            if (await CheckUserExistsAsync(user.Email))
                return null;

            // Lưu mật khẩu trực tiếp (KHÔNG mã hóa)
            user.Password = password;

            // Mặc định trạng thái là kích hoạt
            user.Status = 1;

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        // Đăng nhập người dùng (KIỂM TRA MẬT KHẨU TRỰC TIẾP)
        public async Task<UserAccount> LoginAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null || user.Password != password) // So sánh mật khẩu trực tiếp
            {
                return null; // Sai email hoặc mật khẩu
            }
            return user;
        }
    }
}
