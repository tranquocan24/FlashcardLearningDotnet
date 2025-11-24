using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1.User xóa -> Deck xóa theo (Cascade Delete)
            modelBuilder.Entity<Deck>()
                .HasOne(d => d.Owner)
                .WithMany(u => u.Decks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2.Deck xóa -> Flashcards xóa theo
            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.Deck)
                .WithMany(d => d.Flashcards)
                .HasForeignKey(f => f.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3.User xóa -> Xóa Session. Nhưng Deck xóa -> KHÔNG xóa Session (để Restrict)
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.Deck)
                .WithMany()
                .HasForeignKey(s => s.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
