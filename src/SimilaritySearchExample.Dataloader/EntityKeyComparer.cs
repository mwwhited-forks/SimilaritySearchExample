using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SimilaritySearchExample.Dataloader;

public class EntityKeyComparer<T> : IEqualityComparer<T>
{
    private readonly IEntityType _entityType;

    private Func<T, T, bool>? _cached;

    public EntityKeyComparer(IEntityType entityType) => _entityType = entityType;

    public bool Equals(T? x, T? y)
    {
        if (x == null || y == null) return false;

        if (_cached == null)
        {
            var px = Expression.Parameter(typeof(T), "x");
            var py = Expression.Parameter(typeof(T), "y");

            Expression? expression = null;

            var primaryKey = _entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                var keys = from prop in primaryKey.Properties
                           let propertyInfo = prop.PropertyInfo ?? throw new NotSupportedException($"Unable to find property info for {prop}")
                           select Expression.NotEqual(
                               Expression.Property(px, propertyInfo),
                               Expression.Property(py, propertyInfo)
                           );

                foreach (var key in keys)
                {
                    expression = expression == null ? key : (Expression)Expression.Or(key, expression);
                }
            }

            var realExpression = Expression.Lambda(expression ?? Expression.Constant(false), px, py);
            _cached = (Func<T, T, bool>)realExpression.Compile();
        }

        var result = _cached?.Invoke(x, y) ?? false;
        return result;

        //Expression.Property()

        //var values = from prop in primaryKey.Properties
        //             select new
        //             {
        //                 x = prop.PropertyInfo.GetValue(x),
        //                 y = prop.PropertyInfo.GetValue(y),
        //             };
        //var notMatched = values.Any(i => !i.x.Equals(i.y));
        //return !notMatched;
    }

    public int GetHashCode([DisallowNull] T obj) => obj.GetHashCode();
}
