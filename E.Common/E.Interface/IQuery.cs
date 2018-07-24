using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace E.Interface
{
    //Interfaces and DTO's used in AutoQuery
    public interface IQuery : IMeta
    {
        /// <summary>
        /// How many results to skip
        /// </summary>
        int? Skip { get; set; }

        /// <summary>
        /// How many results to return
        /// </summary>
        int? Take { get; set; }

        /// <summary>
        /// List of fields to sort by, can order by multiple fields and inverse order, e.g: Id,-Amount
        /// </summary>
        string OrderBy { get; set; }

        /// <summary>
        /// List of fields to sort by descending, can order by multiple fields and inverse order, e.g: -Id,Amount
        /// </summary>
        string OrderByDesc { get; set; }

        /// <summary>
        /// Include aggregate data like Total, COUNT(*), COUNT(DISTINCT Field), Sum(Amount), etc
        /// </summary>
        string Include { get; set; }

        /// <summary>
        /// The fields to return
        /// </summary>
        string Fields { get; set; }
    }

    public interface IQueryDb : IQuery { }
    public interface IQueryData : IQuery { }

    public interface IQueryDb<TFrom> : IQueryDb { }
    public interface IQueryDb<TFrom, TInto> : IQueryDb { }

    public interface IQueryData<TFrom> : IQueryData { }
    public interface IQueryData<TFrom, TInto> : IQueryData { }

    public interface IJoin { }
    public interface IJoin<TSource, TJoin1> : IJoin { }
    public interface IJoin<TSource, TJoin1, TJoin2> : IJoin { }
    public interface IJoin<TSource, TJoin1, TJoin2, TJoin3> : IJoin { }
    public interface IJoin<TSource, TJoin1, TJoin2, TJoin3, TJoin4> : IJoin { }

    public interface ILeftJoin<TSource, TJoin1> : IJoin { }
    public interface ILeftJoin<TSource, TJoin1, TJoin2> : IJoin { }
    public interface ILeftJoin<TSource, TJoin1, TJoin2, TJoin3> : IJoin { }
    public interface ILeftJoin<TSource, TJoin1, TJoin2, TJoin3, TJoin4> : IJoin { }

    public enum QueryTerm
    {
        Default = 0,
        And = 1,
        Or = 2,
    }
    public enum ValueStyle
    {
        Single = 0,
        Multiple = 1,
        List = 2,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class QueryDbAttribute : AttributeBase
    {
        public QueryDbAttribute() { }

        public QueryDbAttribute(QueryTerm defaultTerm)
        {
            DefaultTerm = defaultTerm;
        }

        public QueryTerm DefaultTerm { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class QueryDataAttribute : AttributeBase
    {
        public QueryDataAttribute() { }

        public QueryDataAttribute(QueryTerm defaultTerm)
        {
            DefaultTerm = defaultTerm;
        }

        public QueryTerm DefaultTerm { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class QueryDbFieldAttribute : AttributeBase
    {
        public QueryTerm Term { get; set; }
        public string Operand { get; set; }
        public string Template { get; set; }
        public string Field { get; set; }
        public string ValueFormat { get; set; }
        public ValueStyle ValueStyle { get; set; }
        public int ValueArity { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class QueryDataFieldAttribute : AttributeBase
    {
        public QueryTerm Term { get; set; }
        public string Condition { get; set; }
        public string Field { get; set; }
    }

    public abstract class QueryBase : IQuery
    {
        [DataMember(Order = 1)]
        public virtual int? Skip { get; set; }

        [DataMember(Order = 2)]
        public virtual int? Take { get; set; }

        [DataMember(Order = 3)]
        public virtual string OrderBy { get; set; }

        [DataMember(Order = 4)]
        public virtual string OrderByDesc { get; set; }

        [DataMember(Order = 5)]
        public virtual string Include { get; set; }

        [DataMember(Order = 6)]
        public virtual string Fields { get; set; }

        [DataMember(Order = 7)]
        public virtual Dictionary<string, string> Meta { get; set; }
    }

    public abstract class QueryDb<T> : QueryBase, IQueryDb<T>, IReturn<QueryResponse<T>> { }
    public abstract class QueryDb<TFrom, TInto> : QueryBase, IQueryDb<TFrom, TInto>, IReturn<QueryResponse<TInto>> { }

    public abstract class QueryData<T> : QueryBase, IQueryData<T>, IReturn<QueryResponse<T>> { }
    public abstract class QueryData<TFrom, TInto> : QueryBase, IQueryData<TFrom, TInto>, IReturn<QueryResponse<TInto>> { }

    public interface IQueryResponse : IHasResponseStatus, IMeta
    {
        int Offset { get; set; }
        int Total { get; set; }
    }

    [DataContract]
    public class QueryResponse<T> : IQueryResponse
    {
        [DataMember(Order = 1)]
        public virtual int Offset { get; set; }

        /// <summary>
        /// Populate with Include=Total or if registered with: AutoQueryFeature { IncludeTotal = true }
        /// </summary>
        [DataMember(Order = 2)]
        public virtual int Total { get; set; }

        [DataMember(Order = 3)]
        public virtual List<T> Results { get; set; }

        [DataMember(Order = 4)]
        public virtual Dictionary<string, string> Meta { get; set; }

        [DataMember(Order = 5)]
        public virtual ResponseStatus ResponseStatus { get; set; }
    }
}