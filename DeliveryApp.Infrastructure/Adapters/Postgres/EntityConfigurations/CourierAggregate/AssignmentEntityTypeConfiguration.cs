using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;

internal class AssignmentEntityTypeConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("assignments");
        entityTypeBuilder.HasKey(entity => entity.Id);

        // Id
        entityTypeBuilder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        // Location
        entityTypeBuilder
            .ComplexProperty(e => e.Location, p =>
            {
                p.IsRequired();
                p.Property(v => v.X).HasColumnName("location_x").IsRequired();
                p.Property(v => v.Y).HasColumnName("location_y").IsRequired();
            });
        
        // Volume
        entityTypeBuilder
            .ComplexProperty(e => e.Volume, p =>
            {
                p.IsRequired();
                p.Property(v => v.Value).HasColumnName("volume").IsRequired();
            });

        // Status
        entityTypeBuilder
            .ComplexProperty(entity => entity.Status, a =>
            {
                a.IsRequired();
                a.Property(c => c.Name).HasColumnName("status").IsRequired();
            });

        entityTypeBuilder
            .Property(entity => entity.OrderId)
            .HasColumnName("order_id")
            .IsRequired();
        
        entityTypeBuilder
            .Property("CourierId")
            .HasColumnName("courier_id")
            .IsRequired();
    }
}