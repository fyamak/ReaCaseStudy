using Infrastructure.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Postgres.EntityFramework.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Email).IsRequired();
        builder.Property(x => x.FullName).IsRequired();
        builder.Property(x => x.PasswordSalt).IsRequired();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.UserType).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
    }
}
