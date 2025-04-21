using Microsoft.EntityFrameworkCore;

namespace GahuahwaMainService.Models;

public class ComicsContext:DbContext {
    public ComicsContext(DbContextOptions options) : base(options) {
        
    }
    
    public DbSet<Comics> Comics { get; set; } = null!;
}