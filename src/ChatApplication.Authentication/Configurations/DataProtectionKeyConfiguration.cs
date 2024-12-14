using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApplication.Authentication.Configurations;

internal class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.HasKey(key => key.Id);
        builder.Property(key => key.Id).ValueGeneratedOnAdd();

        builder.Property(key => key.FriendlyName).HasColumnType("NVARCHAR(256)").IsRequired(false);
        builder.Property(key => key.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
    }
}