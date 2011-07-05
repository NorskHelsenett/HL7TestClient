using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HL7TestClient
{
    public static class NullSafeObjectPathTraverser
    {
        public static TLeaf Traverse<TRoot, TLeaf>(TRoot root, Expression<Func<TRoot, TLeaf>> path, IList<string> pathToFirstNull)
            where TLeaf : class
        {
            pathToFirstNull.Clear();
            var result = (TLeaf)Traverse(path.Body, root, pathToFirstNull);
            if (result != null)
                pathToFirstNull.Clear();
            return result;
        }

        private static object Traverse(Expression expression, object root, IList<string> pathComponents)
        {
            var expr = expression as MemberExpression;
            if (expr == null)
                return root;
            var sourceNode = Traverse(expr.Expression, root, pathComponents);
            if (sourceNode == null)
                return null;
            pathComponents.Add(expr.Member.Name);
            var prop = expr.Member.DeclaringType.GetProperty(expr.Member.Name);
            return prop.GetValue(sourceNode, new object[0]);
        }
    }
}
