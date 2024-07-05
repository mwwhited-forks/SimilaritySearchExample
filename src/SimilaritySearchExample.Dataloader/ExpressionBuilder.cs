using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using System.Linq.Expressions;

namespace SimilaritySearchExample.Dataloader;

public class ExpressionBuilder : IExpressionBuilder
{
    public IEnumerable ExcludeFrom(IEnumerable haystack, IEnumerable needles, IEntityType entityType)
    {
        var toListInfo = typeof(Enumerable).GetMethod(
        name: nameof(Enumerable.ToList),
        genericParameterCount: 1,
        types: [typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0))]
        )?.MakeGenericMethod(entityType.ClrType) ?? throw new NotSupportedException();

        var comparer = typeof(EntityKeyComparer<>).MakeGenericType(entityType.ClrType).GetConstructor([typeof(IEntityType)])?.Invoke([entityType]);

        var containsInfo = typeof(Enumerable).GetMethod(
                name: nameof(Enumerable.Contains),
                genericParameterCount: 1,
                types: [
                    typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0)),
                    Type.MakeGenericMethodParameter(0),
                    typeof(IEqualityComparer<>).MakeGenericType(Type.MakeGenericMethodParameter(0))
                ]
        )?.MakeGenericMethod(entityType.ClrType) ?? throw new NotSupportedException();

        var existingKeys = toListInfo.Invoke(null, [BuildKeyExpression(entityType).Compile().DynamicInvoke(needles)]);
        var setParameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(entityType.ClrType));
        var itemParameter = Expression.Parameter(entityType.ClrType);

        var predicate = Expression.Lambda(
            Expression.Not(
                Expression.Call(
                    containsInfo,
                    Expression.Constant(existingKeys),
                    itemParameter,
                    Expression.Constant(comparer)
                    )
                ),
            itemParameter
        );

        var whereInfo = typeof(Enumerable).GetMethod(
                name: nameof(Enumerable.Where),
                genericParameterCount: 1,
                types: [
                    typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0)),
                    typeof(Func<,>).MakeGenericType(Type.MakeGenericMethodParameter(0), typeof(bool))
                ]
                )?.MakeGenericMethod(entityType.ClrType) ?? throw new NotSupportedException();

        var missingData = toListInfo.Invoke(null, [whereInfo.Invoke(null, [haystack, predicate.Compile()])]);

        return (IEnumerable)missingData;
    }

    public LambdaExpression BuildKeyExpression(IEntityType entityType)
    {
        var primaryKey = entityType.FindPrimaryKey() ?? throw new NotSupportedException($"Type must have a primary key");

        var sourceParameterExpression = Expression.Parameter(entityType.ClrType);

        var bindingExpressions =
            from property in primaryKey.Properties
            let propertyInfo = property.PropertyInfo ?? throw new NotSupportedException($"No property info for {primaryKey}")
            select Expression.Bind(
                propertyInfo,
                Expression.Property(sourceParameterExpression, propertyInfo)
            );

        var initializer = Expression.MemberInit(
            Expression.New(entityType.ClrType.GetConstructor(Type.EmptyTypes) ??
            throw new NotSupportedException($"Missing empty Constructor for {entityType.ClrType}")
            ),
            bindingExpressions
        );

        var lambdaExpression = Expression.Lambda(
            initializer,
            sourceParameterExpression
            );

        var selectMethod = typeof(Queryable).GetMethod(nameof(Queryable.Select), 2, [
            typeof(IQueryable<>).MakeGenericType(Type.MakeGenericMethodParameter(0)),
            typeof(Expression<>).MakeGenericType(
                typeof(Func<,>).MakeGenericType(Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1))
            )
            ])?
            .MakeGenericMethod(entityType.ClrType, entityType.ClrType)
            ?? throw new NotSupportedException($"Unable to resolve Queryable.Select<S,R>(IQueryable<S>, Expression<Func<S,R>>)");

        var queryParameterExpression = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityType.ClrType));

        var querySelectorExpression = Expression.Call(selectMethod, queryParameterExpression, lambdaExpression);

        var queryLambdaExpression = Expression.Lambda(querySelectorExpression, queryParameterExpression);

        return queryLambdaExpression;
    }

}
