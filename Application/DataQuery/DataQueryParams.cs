using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Application.DataQuery;

public class DataQueryParams<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>>? Expression { get; set; }
    
    public PagingParams? Paging { get; set; }
    
    public SortingParams<TEntity>? Sorting { get; set; }
    
    public List<Expression<Func<TEntity, bool>>>? Filters { get; set; }
    
    public IncludeParams<TEntity>? IncludeParams { get; set; }

    public IQueryable<TEntity> Accumulate(IQueryable<TEntity> set)
    {
        set = ApplyIncludeParams(set);
        set = ApplyFilters(set);

        return set;
    }

    private IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> enumerable)
    {
        if (Filters == null)
        {
            return enumerable;
        }
        return Filters.Aggregate(enumerable, (current, filter) => current.Where(filter));
    }
    
    private IQueryable<TEntity> ApplyIncludeParams(IQueryable<TEntity> enumerable)
    {
        var set = enumerable;
        if (IncludeParams == null)
        {
            return set;
        }
        
        if (IncludeParams.IncludeProperties != null)
        {
            set = IncludeParams.IncludeProperties.Aggregate(set, 
                (current, propertyPath) => current.Include(propertyPath));
        }

        return set;
    }
}