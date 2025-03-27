using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Identity;
namespace Do_An_Web_Hoc.Repositories
{
    public class EFUserAccountRepository :  IUserAccountRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EFUserAccountRepository> _logger;
        private readonly PasswordHasher<UserAccount> _passwordHasher = new PasswordHasher<UserAccount>();


        public EFUserAccountRepository(ApplicationDbContext context , IConfiguration configuration, ILogger<EFUserAccountRepository> logger )
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Tìm người dùng theo email.
        public async Task<UserAccount> GetByEmailAsync(string email)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task UpdateAsync(UserStatus user)
        {
            _context.UserStatus.Update(user);
            await _context.SaveChangesAsync();
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

            // Lưu mật khẩu trực tiếp mã hóa
            user.Password = _passwordHasher.HashPassword(user, password);


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
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                Console.WriteLine($"[ERROR] Đăng nhập thất bại: Email {email} không tồn tại.");
                return null;
            }

            var hasher = new PasswordHasher<UserAccount>();
            var result = hasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
            {
                Console.WriteLine($"[ERROR] Đăng nhập thất bại: Sai mật khẩu cho email {email}.");
                return null;
            }

            Console.WriteLine($"[DEBUG] Đăng nhập thành công: {user.Email} - RoleID: {user.RoleID}");

            // Trả lại thông tin cần thiết (nếu không muốn trả cả password)
            return new UserAccount
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email,
                RoleID = user.RoleID
            };
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
            user.Password = _passwordHasher.HashPassword(user, newPassword);

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
        public async Task UpdateUserAsync(UserAccount user)
        {
            var existingUser = await _context.UserAccounts.FindAsync(user.UserID);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Birthday = user.Birthday;
                existingUser.Status = user.Status;
                existingUser.Address = user.Address;
                existingUser.Image = user.Image;
                await _context.SaveChangesAsync();
            }
        }
        // Phương thức cập nhật thông tin người dùng với ghi log và bắt lỗi
        //public async Task UpdateUserInfoAsync(UserAccount updatedUser)
        //{
        //    try
        //    {
        //        var existingUser = await _context.UserAccounts.FindAsync(updatedUser.UserID);
        //        if (existingUser != null)
        //        {
        //            existingUser.FullName = updatedUser.FullName;
        //            existingUser.Email = updatedUser.Email;
        //            existingUser.PhoneNumber = updatedUser.PhoneNumber;
        //            existingUser.Birthday = updatedUser.Birthday;
        //            existingUser.Address = updatedUser.Address;

        //            if (updatedUser.Image != null)
        //            {
        //                existingUser.Image = updatedUser.Image;  // Nếu có ảnh, cập nhật ảnh
        //            }
        //            else
        //            {
        //                // Nếu không có ảnh, có thể sử dụng ảnh mặc định
        //                existingUser.Image = "/images/default-avatar.png";
        //            }

        //            // Lưu thay đổi
        //            await _context.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            _logger.LogWarning($"User with ID {updatedUser.UserID} not found.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error updating user {updatedUser.UserID}: {ex.Message}");
        //        throw;  // Ném lại lỗi nếu cần thiết
        //    }
        //}
        public async Task UpdateUserInfoAsync(UserAccount updatedUser)
        {
            try
            {
                // Tạo câu lệnh SQL để cập nhật thông tin người dùng
                string sqlQuery = @"
            UPDATE [dbo].[UserAccounts]
            SET 
                FullName = @FullName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                Birthday = @Birthday,
                Address = @Address,
                Image = @Image
            WHERE UserID = @UserID";

                // Tạo các tham số để thay thế trong câu lệnh SQL
                var parameters = new[]
                {
            new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = updatedUser.FullName },
            new SqlParameter("@Email", SqlDbType.NVarChar) { Value = updatedUser.Email },
            new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { Value = updatedUser.PhoneNumber },
            new SqlParameter("@Birthday", SqlDbType.DateTime) { Value = updatedUser.Birthday },
            new SqlParameter("@Address", SqlDbType.NVarChar) { Value = updatedUser.Address },
            new SqlParameter("@Image", SqlDbType.NVarChar) { Value = updatedUser.Image ?? "/images/default-avatar.png" }, // Default image if null
            new SqlParameter("@UserID", SqlDbType.Int) { Value = updatedUser.UserID }
        };

                // Thực thi câu lệnh SQL
                await _context.Database.ExecuteSqlRawAsync(sqlQuery, parameters);

                _logger.LogInformation($"User {updatedUser.UserID} updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {updatedUser.UserID}: {ex.Message}");
                throw; // Ném lại lỗi nếu cần thiết
            }
        }
        public async Task UpdateAsync(UserAccount user)
        {
            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync();
        }


        // Lấy tất cả người dùng
        public async Task<IEnumerable<UserAccount>> GetAllUsersAsync()
        {
            return await _context.UserAccounts.ToListAsync();
        }
        // Lưu danh sách người dùng sau khi mã hóa lại mật khẩu
        public async Task SaveAllUsersAsync(IEnumerable<UserAccount> users)
        {
            _context.UserAccounts.UpdateRange(users);
            await _context.SaveChangesAsync();
        }

        public async Task<UserAccount> GetByIdAsync(int id)
        {
            return await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserID == id);
        }

    }
}

