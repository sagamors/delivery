using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;

internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("orders");
        entityTypeBuilder.HasKey(entity => entity.Id);

        // Id
        entityTypeBuilder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .IsRequired();

        // CourierId
        entityTypeBuilder
            .Property(entity => entity.CourierId)
            .HasColumnName("courier_id")
            .IsRequired(false);
        
        // Volume
        entityTypeBuilder
            .ComplexProperty(entity => entity.Volume, v =>
            {
                v.IsRequired();
                v.Property(c => c.Value).HasColumnName("volume").IsRequired();
            });
        
        // Location
        entityTypeBuilder
            .ComplexProperty(e => e.Location, p =>
            {
                p.IsRequired();
                p.Property(v => v.X).HasColumnName("location_x").IsRequired();
                p.Property(v => v.Y).HasColumnName("location_y").IsRequired();
            });
        
        // Status
        entityTypeBuilder
            .ComplexProperty(entity => entity.Status, a =>
            {
                a.IsRequired();
                a.Property(c => c.Name).HasColumnName("status").IsRequired();
            });
    }
}