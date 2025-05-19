using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data
{
    public class CachingTaskDbContext : DbContext
    {
        public CachingTaskDbContext(DbContextOptions<CachingTaskDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
