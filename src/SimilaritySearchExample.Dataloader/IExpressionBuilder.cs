using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using System.Linq.Expressions;

namespace SimilaritySearchExample.Dataloader;

public interface IExpressionBuilder
{
    IEnumerable ExcludeFrom(IEnumerable haystack, IEnumerable needles, IEntityType entityType);
    LambdaExpression BuildKeyExpression(IEntityType entityType);
}
