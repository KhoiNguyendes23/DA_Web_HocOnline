using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Answers> Answers { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<CategoryStatus> CategoryStatus { get; set; }
        public DbSet<Contacts> Contacts { get; set; }
        public DbSet<CourseContent> CourseContent { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<CourseStatus> CourseStatus { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<Exams> Exams { get; set; }
        public DbSet<Lectures> Lectures { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Quizzes> Quizzes { get; set; }
        public DbSet<Ratings> Ratings { get; set; }
        public DbSet<Results> Results { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserActivities> UserActivities { get; set; }
        public DbSet<UserAnswers> UserAnswers { get; set; }
        public DbSet<UserStatus> UserStatus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Answers - Questions (1-N)
            modelBuilder.Entity<Answers>()
                .HasOne<Questions>()
                .WithMany()
                .HasForeignKey(a => a.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            // Categories - CategoryStatus (1-N)
            modelBuilder.Entity<Categories>()
                .HasOne<CategoryStatus>()
                .WithMany()
                .HasForeignKey(c => c.Status);

            // CourseContent - Courses (1-N)
            modelBuilder.Entity<CourseContent>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(cc => cc.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Courses - Categories (1-N)
            modelBuilder.Entity<Courses>()
                .HasOne<Categories>()
                .WithMany()
                .HasForeignKey(c => c.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            // Courses - CourseStatus (1-N)
            modelBuilder.Entity<Courses>()
                .HasOne<CourseStatus>()
                .WithMany()
                .HasForeignKey(c => c.Status);

            // Documents - Courses (1-N)
            modelBuilder.Entity<Documents>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(d => d.CourseID);

            // Documents - Lectures (1-N)
            modelBuilder.Entity<Documents>()
                .HasOne<Lectures>()
                .WithMany()
                .HasForeignKey(d => d.LectureID);

            // Documents - UserAccounts (1-N) (UploadedBy)
            modelBuilder.Entity<Documents>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(d => d.UploadedBy);

            // Enrollments - Courses (1-N)
            modelBuilder.Entity<Enrollments>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(e => e.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrollments - UserAccounts (1-N)
            modelBuilder.Entity<Enrollments>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Exams - Courses (1-N)
            modelBuilder.Entity<Exams>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(e => e.CourseID);

            // Lectures - Courses (1-N)
            modelBuilder.Entity<Lectures>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(l => l.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            // Payments - UserAccounts (1-N)
            modelBuilder.Entity<Payments>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Questions - Quizzes (1-N)
            modelBuilder.Entity<Questions>()
                .HasOne<Quizzes>()
                .WithMany()
                .HasForeignKey(q => q.QuizID)
                .OnDelete(DeleteBehavior.Cascade);

            // Quizzes - Exams (1-N)
            modelBuilder.Entity<Quizzes>()
                .HasOne<Exams>()
                .WithMany()
                .HasForeignKey(q => q.ExamID);

            // Ratings - Courses (1-N)
            modelBuilder.Entity<Ratings>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(r => r.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            // Ratings - UserAccounts (1-N)
            modelBuilder.Entity<Ratings>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Results - Quizzes (1-N)
            modelBuilder.Entity<Results>()
                .HasOne<Quizzes>()
                .WithMany()
                .HasForeignKey(r => r.QuizID)
                .OnDelete(DeleteBehavior.Cascade);

            // Results - UserAccounts (1-N)
            modelBuilder.Entity<Results>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Reviews - Courses (1-N)
            modelBuilder.Entity<Reviews>()
                .HasOne<Courses>()
                .WithMany()
                .HasForeignKey(r => r.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            // Reviews - UserAccounts (1-N)
            modelBuilder.Entity<Reviews>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // UserAccounts - Roles (1-N)
            modelBuilder.Entity<UserAccount>()
                .HasOne<Roles>()
                .WithMany()
                .HasForeignKey(u => u.RoleID);

            // UserAccounts - UserStatus (1-N)
            modelBuilder.Entity<UserAccount>()
                .HasOne<UserStatus>()
                .WithMany()
                .HasForeignKey(u => u.Status);

            // UserActivities - UserAccounts (1-N)
            modelBuilder.Entity<UserActivities>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserAnswers - Answers (1-N)
            modelBuilder.Entity<UserAnswers>()
                .HasOne<Answers>()
                .WithMany()
                .HasForeignKey(ua => ua.AnswerID);

            // UserAnswers - Questions (1-N)
            modelBuilder.Entity<UserAnswers>()
                .HasOne<Questions>()
                .WithMany()
                .HasForeignKey(ua => ua.QuestionID);

            // UserAnswers - UserAccounts (1-N)
            modelBuilder.Entity<UserAnswers>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(ua => ua.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
