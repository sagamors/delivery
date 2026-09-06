using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox;

internal class OutboxEntityTypeConfiguration : IEntityTypeConfiguration<Entities.OutboxMessage>
{
    public void Configure(EntityTypeBuilder<Entities.OutboxMessage> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("outbox");
        entityTypeBuilder.HasKey(entity => entity.Id);

        // Id
        entityTypeBuilder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        // Type
        entityTypeBuilder
            .Property(entity => entity.Type)
            .HasColumnName("type")
            .IsRequired();

        // Payload
        entityTypeBuilder
            .Property(entity => entity.Payload)
            .HasColumnName("content")
            .IsRequired();

        // OccurredOnUtc
        entityTypeBuilder
            .Property(entity => entity.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        // ProcessedOnUtc
        entityTypeBuilder
            .Property(entity => entity.ProcessedOnUtc)
            .HasColumnName("processed_on_utc")
            .IsRequired(false);
    }
}