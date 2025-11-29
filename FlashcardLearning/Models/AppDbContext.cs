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
        public DbSet<Folder> Folders { get; set; }
        public DbSet<DictionaryEntry> DictionaryEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. USER -> DECK (Cascade Delete)
            modelBuilder.Entity<Deck>()
                .HasOne(d => d.Owner)
                .WithMany(u => u.Decks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. DECK -> FLASHCARD (Cascade Delete)
            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.Deck)
                .WithMany(d => d.Flashcards)
                .HasForeignKey(f => f.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. USER -> STUDY SESSION (No Action)
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // 4. DECK -> STUDY SESSION (Cascade)
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.Deck)
                .WithMany()
                .HasForeignKey(s => s.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. User xóa -> Folder xóa theo (Cascade Delete)
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Folder xóa -> Deck KHÔNG xóa, chỉ set FolderId = NULL
            modelBuilder.Entity<Deck>()
                .HasOne(d => d.Folder)
                .WithMany(f => f.Decks)
                .HasForeignKey(d => d.FolderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // 6. DICTIONARY ENTRY - Unique Word
            modelBuilder.Entity<DictionaryEntry>()
                .HasIndex(d => d.Word)
                .IsUnique();
        }
    }
}
