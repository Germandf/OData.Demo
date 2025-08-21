using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public static class QueryableExtensions
{
    public static IQueryable<TDto> ProjectTo<TSource, TDto>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDto>> projection)
        where TSource : class
        => query.AsNoTracking()
                .AsExpandable()
                .Select(projection);
}
