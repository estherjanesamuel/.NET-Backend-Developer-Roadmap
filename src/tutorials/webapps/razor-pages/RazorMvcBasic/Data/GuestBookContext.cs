using Microsoft.EntityFrameworkCore;

namespace RazorMvcBasic.Data;

public class GuestBookContext : DbContext
{
    public GuestBookContext(
        DbContextOptions options
    ) : base(options)
    {
        
    }
    public DbSet<Entry> Entries { get; set; }
}