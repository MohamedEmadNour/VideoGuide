using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VideoGuide.Models;

public partial class VideoGuideContext : DbContext
{
    public VideoGuideContext()
    {
    }

    public VideoGuideContext(DbContextOptions<VideoGuideContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupTag> GroupTags { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<VideoTag> VideoTags { get; set; }

    public virtual DbSet<Video_Fav> Video_Favs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_CI_AS");

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupID).HasName("PK__Groups__149AF30AFED2749C");

            entity.Property(e => e.Lantin_GroupName).HasMaxLength(255);
            entity.Property(e => e.Local_GroupName).HasMaxLength(255);
        });

        modelBuilder.Entity<GroupTag>(entity =>
        {
            entity.HasKey(e => e.GroupTagID).HasName("PK__GroupTag__0E20B580D677E04D");

            entity.ToTable("GroupTag");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupTags)
                .HasForeignKey(d => d.GroupID)
                .HasConstraintName("FK__GroupTag__GroupI__2E1BDC42");

            entity.HasOne(d => d.Tag).WithMany(p => p.GroupTags)
                .HasForeignKey(d => d.TagID)
                .HasConstraintName("FK__GroupTag__TagID__2F10007B");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagID).HasName("PK__Tag__657CFA4CB0AA4978");

            entity.ToTable("Tag");

            entity.Property(e => e.Lantin_TagName).HasMaxLength(255);
            entity.Property(e => e.Local_TagName).HasMaxLength(255);
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.UserGroupID).HasName("PK__UserGrou__FA5A61E066F22735");

            entity.ToTable("UserGroup");

            entity.Property(e => e.Id).HasMaxLength(450);

            entity.HasOne(d => d.Group).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.GroupID)
                .HasConstraintName("FK__UserGroup__Group__29572725");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.Id)
                .HasConstraintName("FK_UserGroup_AspNetUsers");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.VideoID).HasName("PK__Video__BAE5124AFE7663F4");

            entity.ToTable("Video");

            entity.Property(e => e.Video_Lantin_Title).HasMaxLength(255);
            entity.Property(e => e.Video_Local_Tiltle).HasMaxLength(255);
        });

        modelBuilder.Entity<VideoTag>(entity =>
        {
            entity.HasKey(e => e.VideoTagID).HasName("PK__VideoTag__8A32F004A061C8B8");

            entity.ToTable("VideoTag");

            entity.HasOne(d => d.Tag).WithMany(p => p.VideoTags)
                .HasForeignKey(d => d.TagID)
                .HasConstraintName("FK__VideoTag__TagID__35BCFE0A");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoTags)
                .HasForeignKey(d => d.VideoID)
                .HasConstraintName("FK__VideoTag__VideoI__34C8D9D1");
        });

        modelBuilder.Entity<Video_Fav>(entity =>
        {
            entity.HasKey(e => e.Video_Fav1);

            entity.ToTable("Video_Fav");

            entity.Property(e => e.Video_Fav1).HasColumnName("Video_Fav");
            entity.Property(e => e.Id).HasMaxLength(450);

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Video_Favs)
                .HasForeignKey(d => d.Id)
                .HasConstraintName("FK_Video_Fav_AspNetUsers");

            entity.HasOne(d => d.Video).WithMany(p => p.Video_Favs)
                .HasForeignKey(d => d.VideoID)
                .HasConstraintName("FK_Video_Fav_Video");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
