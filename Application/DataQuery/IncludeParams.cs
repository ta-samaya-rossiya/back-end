using System.Linq.Expressions;

namespace Application.DataQuery;

public class IncludeParams<TEntity>
{
    public List<Expression<Func<TEntity, object?>>>? IncludeProperties { get; set; }
}