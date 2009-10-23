using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Globalization;
using ClickCoerce.Providers;
using Amazon.ECS;

namespace ClickCoerce.Providers.Amazon
{
    public enum ResultFetchMode { Sync, Async }

    public class AmazonContext
    {
        static AmazonContext()
        {
            AwsAccessKeyId = String.Empty;
            AssociateTag = String.Empty;
            Culture = new CultureInfo("en-US");
            Region = new RegionInfo("US");

            DefaultSearchIndex = "Electronics";
            MinimumItemsPage = 1;
            MaximumItemsPage = 400;
            ItemPagesPerRequest = 2;
            MaxItemPagesPerRequest = 2;
            ItemsPerPage = 10;
            FetchMode = ResultFetchMode.Async;
            MaxWorkers = 8;
        }

        public AmazonContext()
        {
            CriteriaBuilder = new CriteriaBuilder<AmazonCriteria>();
            CriteriaResolver = new CriteriaResolver<AmazonCriteria>();
        }

        internal ICriteriaBuilder<AmazonCriteria> CriteriaBuilder { get; set; }
        internal ICriteriaResolver<AmazonCriteria> CriteriaResolver { get; set; }

        public static string AwsAccessKeyId { get; set; }
        public static string AssociateTag { get; set; }
        public static CultureInfo Culture { get; set; }
        public static RegionInfo Region { get; set; }
        public static string DefaultSearchIndex { get; set; }
        public static ResultFetchMode FetchMode { get; set; }

        internal static AmazonECSLocale EcsLocale
        {
            get
            {
                switch (Region.TwoLetterISORegionName)
                {
                    case "CA":
                        return AmazonECSLocale.CA;
                    case "DE":
                        return AmazonECSLocale.DE;
                    case "FR":
                        return AmazonECSLocale.FR;
                    case "JP":
                        return AmazonECSLocale.JP;
                    case "UK":
                        return AmazonECSLocale.UK;
                    case "US":
                    default:
                        return AmazonECSLocale.US;
                }
            }
        }

        internal static int MinimumItemsPage { get; private set; }
        internal static int MaximumItemsPage { get; private set; }
        internal static int ItemPagesPerRequest { get; private set; }
        internal static int MaxItemPagesPerRequest { get; private set;}
        internal static int ItemsPerPage { get; private set; }
        internal static int MaxWorkers { get; private set; }
            

        internal object Execute<TElement>(Expression expression, bool isEnumerable)
        {
            InnermostWhereFinder whereFinder = new InnermostWhereFinder();
            MethodCallExpression whereExpression = whereFinder.GetInnermostWhere(expression);
            LambdaExpression lambdaExpression = (LambdaExpression)((UnaryExpression)(whereExpression.Arguments[1])).Operand;
            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            

            IQueryable<TElement> queryableData;
            AmazonExpressionCriteriaVisitor<TElement> criteriaVisitor = new AmazonExpressionCriteriaVisitor<TElement>(CriteriaBuilder, CriteriaResolver);
            ExpressionCacheModifier<TElement> expressionCacheVisitor = new ExpressionCacheModifier<TElement>();

            if (expressionCacheVisitor.GetCache(expression, out queryableData))
            {
                if (queryableData == null)
                {
                    queryableData = AmazonResult<TElement>.Fetch(criteriaVisitor.GetCriteria(expression)).AsQueryable<TElement>();
                    expressionCacheVisitor.SetCache(expression, queryableData);
                }
            }
            else
            {
                queryableData = AmazonResult<TElement>.Fetch(criteriaVisitor.GetCriteria(expression)).AsQueryable<TElement>();
            }

            ExpressionDataModifier<TElement> expressionModifier = new ExpressionDataModifier<TElement>(queryableData);
            Expression newExpression = expressionModifier.CopyAndModify(expression);

            if (isEnumerable)
            {
                return queryableData.Provider.CreateQuery(newExpression);
            }
            else
            {
                return queryableData.Provider.Execute(newExpression);
            }
        }
    }
}
