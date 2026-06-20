using System.Linq.Expressions;

namespace YourApp.Domain.Interfaces
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>> Criteria { get; protected set; } // ✅ Changed to protected set
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; set; }
        public bool IsCountQuery { get; protected set; } // ✅ Changed to protected set

        protected BaseSpecification()
        {
            IsCountQuery = false;
        }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
            IsCountQuery = false;
        }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        // ✅ Method to create a count query specification
        public virtual BaseSpecification<T> AsCountQuery()
        {
            IsCountQuery = true;
            // For count query, we don't need ordering or includes
            OrderBy = null;
            OrderByDescending = null;
            IsPagingEnabled = false;
            return this;
        }
    }
}