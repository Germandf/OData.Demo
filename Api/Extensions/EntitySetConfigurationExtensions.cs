using Microsoft.OData.ModelBuilder;

public static class EntitySetConfigurationExtensions
{
    public static EntitySetConfiguration<TEntityType> Page<TEntityType>(
        this EntitySetConfiguration<TEntityType> entitySet,
        ODataPage settings) where TEntityType : class
    {
        entitySet.EntityType.Page(
            maxTopValue: settings.MaxTop,
            pageSizeValue: settings.PageSize);

        return entitySet;
    }
}

public class ODataPage
{
    public int? MaxTop { get; set; }
    public int? PageSize { get; set; }
}
