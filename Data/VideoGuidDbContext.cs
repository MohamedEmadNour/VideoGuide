using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace VideoGuide.Data
{
    public class VideoGuidDbContext : IdentityDbContext<ApplicationUser>
    {
        public VideoGuidDbContext(DbContextOptions<VideoGuidDbContext> options)
        : base(options)
        {
        }
        public virtual DbSet<ApplicationUser> ApplicationUser { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>();
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey }); // Define the primary key
            });
        }

    }
}
