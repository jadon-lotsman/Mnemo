using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Mnemo.Infrastructure.Conventions
{
    public class GuidV7DefaultValueConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Guid) && property.ValueGenerated == ValueGenerated.OnAdd)
                    {
                        property.SetDefaultValueSql("uuidv7()", fromDataAnnotation: false);
                    }
                }
            }
        }
    }
}
