using LMSTT.Models;
using Microsoft.EntityFrameworkCore;
namespace LMSTT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<AcademicYear> AcademicYear { get; set; }
        public DbSet<Quizzes> Quizzes { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<QuizSubmissions> QuizSubmissions { get; set; }
        public DbSet<Assignments> Assignments { get; set; }
        public DbSet<AssignmentSubmissions> AssignmentSubmissions { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<DiscussionMessage> DiscussionMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Password).IsRequired();
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Description).IsRequired();
            });

            // Configure UserRole entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Code).IsRequired();
                entity.HasOne(c => c.Teacher)
                    .WithMany()
                    .HasForeignKey(c => c.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Enrollment entity
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Student)
                    .WithMany(u => u.Enrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CourseMaterial entity
            modelBuilder.Entity<CourseMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.StoredFileName).IsRequired();
                entity.Property(e => e.ContentType).IsRequired();
                entity.Property(e => e.UploadedAt).IsRequired();
                entity.HasOne(cm => cm.Course)
                    .WithMany(c => c.Materials)
                    .HasForeignKey(cm => cm.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Quizzes entity
            modelBuilder.Entity<Quizzes>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.HasOne(q => q.Course)
                    .WithMany()
                    .HasForeignKey(q => q.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(q => q.Creator)
                    .WithMany()
                    .HasForeignKey(q => q.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Questions entity
            modelBuilder.Entity<Questions>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionOptions).IsRequired();
                entity.Property(e => e.CorrectAnswer).IsRequired();
                entity.Property(e => e.Points).HasDefaultValue(1);
                entity.HasOne(q => q.Quiz)
                    .WithMany(qz => qz.Questions)
                    .HasForeignKey(q => q.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure QuizSubmissions entity
            modelBuilder.Entity<QuizSubmissions>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Answer).IsRequired();
                entity.Property(e => e.SubmittedAt).IsRequired();
                entity.Property(e => e.StartedAt).IsRequired();
                entity.Property(e => e.CompletionStatus).IsRequired();
                entity.HasOne(qs => qs.Quiz)
                    .WithMany()
                    .HasForeignKey(qs => qs.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(qs => qs.Student)
                    .WithMany()
                    .HasForeignKey(qs => qs.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(qs => qs.Question)
                    .WithMany()
                    .HasForeignKey(qs => qs.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Assignments entity
            modelBuilder.Entity<Assignments>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CourseId).IsRequired();
                
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                    .IsRequired();
                
                entity.Property(e => e.CreatedBy)
                    .IsRequired();
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
                
                entity.Property(e => e.DueDate)
                    .IsRequired();

                entity.HasOne(a => a.Course)
                    .WithMany(c => c.Assignments)
                    .HasForeignKey(a => a.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Creator)
                    .WithMany()
                    .HasForeignKey(a => a.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AssignmentSubmissions entity
            modelBuilder.Entity<AssignmentSubmissions>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.AssignmentId).IsRequired();
                entity.Property(e => e.StudentId).IsRequired();
                entity.Property(e => e.SubmittedText).IsRequired();
                entity.Property(e => e.SubmittedFile).IsRequired(false);
                entity.Property(e => e.StoredFileName).IsRequired(false);
                
                entity.Property(e => e.CompletionStatus)
                    .IsRequired()
                    .HasConversion<int>();
                
                entity.Property(e => e.ActionStatus)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasDefaultValue(ActionStatus.NoAction);
                
                entity.Property(e => e.SubmittedAt).IsRequired();

                entity.HasOne(s => s.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(s => s.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Student)
                    .WithMany()
                    .HasForeignKey(s => s.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            var adminUser = new User
            {
                Id = 1,
                FullName = "Admin User",
                Email = "admin@lms.com",
                Password = "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=", // SHA256 hash for 'Admin@123'
                CreatedAt = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            var teacherUser = new User
            {
                Id = 2,
                FullName = "Test Teacher",
                Email = "teacher@lms.com",
                Password = "0EHD08pO1kxbVMXYB72aC9LWrjYJ7NLQasOD20STYOE=", // SHA256 hash for 'Teacher@123'
                CreatedAt = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<User>().HasData(adminUser, teacherUser);

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Administrator role" },
                new Role { Id = 2, Name = "Teacher", Description = "Teacher role" },
                new Role { Id = 3, Name = "Student", Description = "Student role" }
            );

            // Seed user roles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1 }, // Admin role for admin user
                new UserRole { Id = 2, UserId = 2, RoleId = 2 }  // Teacher role for teacher user
            );
        }
    }
}
