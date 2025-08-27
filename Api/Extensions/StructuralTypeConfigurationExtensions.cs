using Microsoft.OData.ModelBuilder;

public static class StructuralTypeConfigurationExtensions
{
    public static StructuralTypeConfiguration<TStructuralType> Page<TStructuralType>(
        this StructuralTypeConfiguration<TStructuralType> structuralType,
        ODataPage settings) where TStructuralType : class
    {
        return structuralType.Page(
            maxTopValue: settings.MaxTop,
            pageSizeValue: settings.PageSize);
    }
}

public class ODataPage
{
    public int? MaxTop { get; set; }
    public int? PageSize { get; set; }
}
