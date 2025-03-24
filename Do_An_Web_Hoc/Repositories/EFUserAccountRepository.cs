using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
namespace Do_An_Web_Hoc.Repositories
{
    public class EFUserAccountRepository :  IUserAccountRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public EFUserAccountRepository(ApplicationDbContext context , IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            var user = await _context.UserAccounts
                .Where(u => u.Email == email && u.Password == password)
                .Select(u => new UserAccount
                {
                    UserID = u.UserID,
                    UserName = u.UserName,
                    Email = u.Email,
                    RoleID = u.RoleID
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                Console.WriteLine($"[ERROR] Đăng nhập thất bại: Email {email} không tồn tại hoặc mật khẩu sai.");
                return null;
            }

            Console.WriteLine($"[DEBUG] Đăng nhập thành công: {user.Email} - RoleID: {user.RoleID}");
            return user;
        }
        // (1) Gửi mã OTP đến email người dùng
        public async Task<bool> SendOTPAsync(string email)
        {
            var user = await GetByEmailAsync(email);
            if (user == null)
                return false;

            // Tạo mã OTP ngẫu nhiên 6 chữ số
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            user.ResetToken = otp;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10); // Hiệu lực 10 phút
            await _context.SaveChangesAsync();

            // Gửi OTP qua Email
            await SendOTPEmailAsync(email, otp);

            return true;
        }
        // (2) Kiểm tra OTP hợp lệ không
        public async Task<bool> VerifyOTPAsync(string email, string otp)
        {
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.ResetToken == otp &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            return user != null;
        }
        // (3) Reset mật khẩu mới bằng OTP
        public async Task<bool> ResetPasswordByOTPAsync(string email, string newPassword)
        {
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.ResetTokenExpiry > DateTime.UtcNow);
            Console.WriteLine($"[SUCCESS] Mật khẩu của {email} đã được cập nhật.");
            if (user == null)
            {
                Console.WriteLine($"[ERROR] Không tìm thấy tài khoản hoặc OTP đã hết hạn cho email: {email}");
                return false;
            }
            // Kiểm tra xem ResetToken có null không (nếu bạn có lưu ResetToken khi gửi OTP)
            if (string.IsNullOrEmpty(user.ResetToken))
            {
                Console.WriteLine($"[ERROR] ResetToken không hợp lệ hoặc đã bị xóa.");
                return false;
            }
            // Không dùng mã hóa mật khẩu nếu chưa cần
            user.Password = newPassword;

            // Xóa thông tin OTP sau khi đổi mật khẩu thành công
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();
            Console.WriteLine($"[SUCCESS] Mật khẩu của {email} đã được cập nhật.");
            return true;
        }

        // Hàm hỗ trợ gửi OTP bằng SMTP
        private async Task SendOTPEmailAsync(string email, string otp)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Sender"], "Website Support"),
                Subject = "Mã OTP xác thực đổi mật khẩu",
                Body = $"Mã OTP của bạn là: <strong>{otp}</strong>. OTP có hiệu lực trong 10 phút.",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Sender"],
                    _configuration["EmailSettings:Password"]
                ),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

       
    }
}

