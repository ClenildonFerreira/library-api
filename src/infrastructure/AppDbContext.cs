using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;

using LibraryApi.Entities;

namespace LibraryApi.Infrastructure;using LibraryApi.Infrastructure.EntityConfigurations;



public class AppDbContext : DbContextnamespace LibraryApi.Infrastructure;

{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }public class AppDbContext : DbContext

{

    // DbSets serão adicionados posteriormente    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

}
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}