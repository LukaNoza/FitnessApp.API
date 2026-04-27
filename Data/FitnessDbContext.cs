using FitnessApp.API.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FitnessApp.API.Data
{
    // Entity Framework Core DbContext - ხიდი აპლიკაციასა და მონაცემთა ბაზას შორის
    public class FitnessDbContext : DbContext
    {
        // კონსტრუქტორი - იღებს კონფიგურაციას (connection string)
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // DbSets - ესენი არიან ცხრილები მონაცემთა ბაზაში
        // ==========================================

        public DbSet<Notification> Notifications { get; set; }

        // მომხმარებლების ცხრილი (კლიენტები, ტრენერები, ადმინები)
        public DbSet<User> Users { get; set; }

        // ტრენერების დამატებითი ინფორმაცია (1:1 კავშირი Users-თან)
        public DbSet<Trainer> Trainers { get; set; }

        // დარბაზების ცხრილი
        public DbSet<Gym> Gyms { get; set; }

        // კავშირი ტრენერებსა და დარბაზებს შორის (რომელ ტრენერი სად მუშაობს)
        public DbSet<TrainerGym> TrainerGyms { get; set; }

        // ვარჯიშის ტიპები (Crossfit, Yoga, Bodybuilding...)
        public DbSet<WorkoutType> WorkoutTypes { get; set; }

        // კავშირი ტრენერებსა და ვარჯიშის ტიპებს შორის
        public DbSet<TrainerWorkoutType> TrainerWorkoutTypes { get; set; }

        // ჯავშნების ცხრილი
        public DbSet<Booking> Bookings { get; set; }

        // გრაფიკის ცხრილი - ხელმისაწვდომი სლოტები
        public DbSet<Schedule> Schedules { get; set; }

        // შეფასებების/კომენტარების ცხრილი
        public DbSet<Review> Reviews { get; set; }

        // Fluent API - ცხრილების კონფიგურაცია
        // აქ განისაზღვრება: პირველადი გასაღებები, ველების ტიპები, უნიკალურობა, Foreign Keys
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // User Configuration - მომხმარებლების ცხრილის კონფიგურაცია
            // ==========================================
            modelBuilder.Entity<User>(entity =>
            {
                // პირველადი გასაღები (Primary Key)
                entity.HasKey(e => e.Id);

                // სახელი - აუცილებელია, მაქსიმუმ 50 სიმბოლო
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                // გვარი - აუცილებელია, მაქსიმუმ 50 სიმბოლო
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                // ელ.ფოსტა - აუცილებელია, მაქსიმუმ 100 სიმბოლო
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                // ელ.ფოსტა უნიკალური უნდა იყოს (არ შეიძლება ორი მომხმარებელი ერთი იმეილით)
                entity.HasIndex(e => e.Email)
                    .IsUnique();

                // ტელეფონის ნომერი - მაქსიმუმ 20 სიმბოლო, არაა აუცილებელი
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                // მომხმარებლის ტიპი (Client, Trainer, Admin) - ნაგულისხმევია "Client"
                entity.Property(e => e.UserType)
                    .HasMaxLength(20)
                    .HasDefaultValue("Client");

                // აქტიურია თუ არა - ნაგულისხმევია true
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // შექმნის თარიღი - ავტომატურად იწერება UTC დრო
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ==========================================
            // Gym Configuration - დარბაზების ცხრილის კონფიგურაცია
            // ==========================================
            modelBuilder.Entity<Gym>(entity =>
            {
                entity.HasKey(e => e.Id);

                // დარბაზის სახელი - აუცილებელია
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // მისამართი - აუცილებელია
                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(200);

                // ქალაქი - აუცილებელია
                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(50);

                // ტელეფონი - აუცილებელია
                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                // აქტიურია თუ არა (რბილი წაშლისთვის)
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // შექმნის თარიღი
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ==========================================
            // Trainer Configuration - ტრენერების ცხრილის კონფიგურაცია
            // ==========================================
            modelBuilder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.Id);

                // სპეციალიზაცია (Crossfit, Bodybuilding...) - აუცილებელია
                entity.Property(e => e.Specialization)
                    .IsRequired()
                    .HasMaxLength(100);

                // საათობრივი გადასახადი - decimal ტიპი (10 ციფრი, 2 ათწილადი)
                entity.Property(e => e.HourlyRate)
                    .HasColumnType("decimal(10,2)");

                // რეიტინგი - 0.00-დან 5.00-მდე
                entity.Property(e => e.Rating)
                    .HasColumnType("decimal(3,2)")
                    .HasDefaultValue(0);

                // შექმნის თარიღი
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Trainer - User Relationship (1:1)
                // ერთ ტრენერს აქვს ერთი მომხმარებელი (User), პირიქითაც ასე
                entity.HasOne(e => e.User)
                    .WithOne(e => e.Trainer)  // User-ს აქვს ერთი Trainer
                    .HasForeignKey<Trainer>(e => e.UserId)  // Foreign Key არის Trainer.UserId
                    .OnDelete(DeleteBehavior.Cascade);  // თუ User წაიშლება, Trainer-იც წაიშლება
            });

            // ==========================================
            // TrainerGym Configuration - ტრენერ-დარბაზის კავშირი
            // ==========================================
            modelBuilder.Entity<TrainerGym>(entity =>
            {
                entity.HasKey(e => e.Id);

                // სამუშაო დღეები (მაგ: "Mon,Tue,Wed,Thu,Fri")
                entity.Property(e => e.WorkDays)
                    .IsRequired()
                    .HasMaxLength(50);

                // ერთი ტრენერი ერთ დარბაზში მხოლოდ ერთხელ შეიძლება იყოს
                entity.HasIndex(e => new { e.TrainerId, e.GymId })
                    .IsUnique();

                // TrainerGym - Trainer Relationship (ბევრი TrainerGym -> ერთი Trainer)
                entity.HasOne(e => e.Trainer)
                    .WithMany(e => e.TrainerGyms)  // Trainer-ს შეიძლება ჰყავდეს ბევრი TrainerGym
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);  // Trainer-ის წაშლისას მისი კავშირებიც წაიშლება

                // TrainerGym - Gym Relationship
                entity.HasOne(e => e.Gym)
                    .WithMany(e => e.TrainerGyms)  // Gym-ს შეიძლება ჰყავდეს ბევრი ტრენერი
                    .HasForeignKey(e => e.GymId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // WorkoutType Configuration - ვარჯიშის ტიპები
            // ==========================================
            modelBuilder.Entity<WorkoutType>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ვარჯიშის სახელი (Crossfit, Yoga...)
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                // URL-ში გამოსაყენებელი ვერსია (crossfit, yoga...)
                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(50);

                // სახელი უნიკალური უნდა იყოს
                entity.HasIndex(e => e.Name)
                    .IsUnique();

                // Slug უნიკალური უნდა იყოს
                entity.HasIndex(e => e.Slug)
                    .IsUnique();

                // კატეგორია (Strength, Cardio, Flexibility...)
                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50);

                // აქტიურია თუ არა
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ==========================================
            // TrainerWorkoutType Configuration - ტრენერ-ვარჯიშის კავშირი (მრავალ-მრავალზე)
            // ==========================================
            modelBuilder.Entity<TrainerWorkoutType>(entity =>
            {
                // Composite Key - ორივე ველი ერთად არის პირველადი გასაღები
                entity.HasKey(e => new { e.TrainerId, e.WorkoutTypeId });

                // ფასის მოდიფიკატორი (მაგ: 1.2 = 20%-ით ძვირი)
                entity.Property(e => e.PriceModifier)
                    .HasColumnType("decimal(3,2)")
                    .HasDefaultValue(1.0m);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // TrainerWorkoutType - Trainer Relationship
                entity.HasOne(e => e.Trainer)
                    .WithMany(e => e.TrainerWorkoutTypes)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // TrainerWorkoutType - WorkoutType Relationship
                entity.HasOne(e => e.WorkoutType)
                    .WithMany(e => e.TrainerWorkoutTypes)
                    .HasForeignKey(e => e.WorkoutTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // Booking Configuration - ჯავშნები
            // ==========================================
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);

                // სტატუსი (Pending, Confirmed, Completed, Cancelled, NoShow)
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                // ჯამური თანხა (decimal ტიპი ფულისთვის)
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Booking - Client (User) Relationship
                // Client (მომხმარებელი) - შეიძლება ჰქონდეს ბევრი ჯავშანი
                entity.HasOne(e => e.Client)
                    .WithMany(e => e.ClientBookings)  // User-ს აქვს ClientBookings კოლექცია
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);  // არ წაშალო User თუ აქვს ჯავშნები

                // Booking - Trainer Relationship
                entity.HasOne(e => e.Trainer)
                    .WithMany(e => e.Bookings)  // Trainer-ს აქვს ბევრი ჯავშანი
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Restrict);  // არ წაშალო Trainer თუ აქვს ჯავშნები

                // Booking - Gym Relationship
                entity.HasOne(e => e.Gym)
                    .WithMany(e => e.Bookings)  // Gym-ს აქვს ბევრი ჯავშანი
                    .HasForeignKey(e => e.GymId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // Schedule Configuration - გრაფიკი (ხელმისაწვდომი სლოტები)
            // ==========================================
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);

                // არის თუ არა ეს სლოტი სრულად დაჯავშნილი
                entity.Property(e => e.IsBooked)
                    .HasDefaultValue(false);

                // მაქსიმუმ რამდენ კლიენტს შეუძლია ერთ სლოტზე ჯავშანი
                entity.Property(e => e.MaxClients)
                    .HasDefaultValue(1);

                // ამჟამად რამდენი ჯავშანია ამ სლოტზე
                entity.Property(e => e.CurrentBookings)
                    .HasDefaultValue(0);

                // აქტიურია თუ არა (რბილი წაშლა)
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Schedule - TrainerGym Relationship
                // თითოეული სლოტი ეკუთვნის კონკრეტულ ტრენერ-დარბაზის კავშირს
                entity.HasOne(e => e.TrainerGym)
                    .WithMany(e => e.Schedules)  // TrainerGym-ს შეიძლება ჰქონდეს ბევრი სლოტი
                    .HasForeignKey(e => e.TrainerGymId)
                    .OnDelete(DeleteBehavior.Cascade);  // TrainerGym-ის წაშლისას მისი სლოტებიც წაიშლება
            });

            // ==========================================
            // Review Configuration - შეფასებები/კომენტარები
            // ==========================================
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);

                // რეიტინგი 1-დან 5-მდე - tinyint (1-255)
                entity.Property(e => e.Rating)
                    .IsRequired()
                    .HasColumnType("int");

                // კომენტარი - მაქსიმუმ 1000 სიმბოლო
                entity.Property(e => e.Comment)
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Review - Booking Relationship (1:1)
                entity.HasOne(e => e.Booking)
                    .WithOne(e => e.Review)
                    .HasForeignKey<Review>(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Review - Client Relationship
                entity.HasOne(e => e.Client)
                    .WithMany(e => e.Reviews)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Review - Trainer Relationship
                entity.HasOne(e => e.Trainer)
                    .WithMany(e => e.Reviews)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}




//📊 მნიშვნელოვანი ცნებები
//ტერმინი ახსნა
//DbSet	წარმოადგენს ცხრილს მონაცემთა ბაზაში
//HasKey()	ადგენს პირველად გასაღებს (Primary Key)
//HasIndex().IsUnique()	ადგენს უნიკალურობის კონსტრეინტს
//HasDefaultValue()	ნაგულისხმევი მნიშვნელობა C#-დან
//HasDefaultValueSql()	ნაგულისხმევი მნიშვნელობა SQL-დან
//HasColumnType()	ადგენს SQL ტიპს (decimal, nvarchar...)
//HasMaxLength()	მაქსიმალური სიგრძე string ველებისთვის
//OnDelete(DeleteBehavior)	რა მოხდეს მშობელი ჩანაწერის წაშლისას
//HasOne() / WithOne()	1:1 ურთიერთობა
//HasOne() / WithMany()   1:Many ურთიერთობა
//HasForeignKey()	ადგენს Foreign Key-ს
//🔑 DeleteBehavior ტიპები
//ტიპი	ახსნა
//Cascade	მშობლის წაშლისას შვილიც წაიშლება
//Restrict	ვერ წაშლი მშობელს თუ შვილი არსებობს
//SetNull	მშობლის წაშლისას Foreign Key ხდება NULL
//NoAction	არაფერს აკეთებს (SQL Server-ში იგივეა რაც Restrict)