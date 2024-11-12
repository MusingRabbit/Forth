using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RockRaiders.Util
{
    public class CopyPropertiesAction<T, TSrc> where T : class where TSrc : class
    {
        private T target;
        private TSrc copySource;
        private List<PropertyInfo> excludedProperties;
        private List<Func<object, bool>> queries;

        /// <summary>
        /// Gets the result of the current action
        /// </summary>
        public T Result
        {
            get
            {
                return this.Create();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">The object reference values are to be copied TO</param>
        /// <param name="source">the object reference values are to be copied FROM</param>
        public CopyPropertiesAction(T target, TSrc source)
        {
            this.target = target;
            this.copySource = source;
            this.excludedProperties = new List<PropertyInfo>();
            this.queries = new List<Func<object, bool>>();
        }

        /// <summary>
        /// Method for clamping specified properties within the target object so they are not written to
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="propSelector">Expression for selecting property to be clamped</param>
        /// <returns>The current instance of <see cref="CopyPropertiesAction{T, TSrc}"/></returns>
        public CopyPropertiesAction<T, TSrc> Clamp<TProp>(Expression<Func<TSrc, TProp>> propSelector)
        {
            var memberExpression = (MemberExpression)propSelector.Body;
            excludedProperties.Add((PropertyInfo)memberExpression.Member);
            return this;
        }

        /// <summary>
        /// Method for appending a Where clause to the action
        /// </summary>
        /// <param name="query"></param>
        /// <returns>The current instance of <see cref="CopyPropertiesAction{T, TSrc}"/></returns>
        public CopyPropertiesAction<T, TSrc> Where(Func<object, bool> query)
        {
            this.queries.Add(query);
            return this;
        }

        /// <summary>
        /// Creates a where clause to filter out properties from the source object where the property has a null, zero or empty value
        /// </summary>
        /// <returns>The current instance of <see cref="CopyPropertiesAction{T, TSrc}"/></returns>
        public CopyPropertiesAction<T, TSrc> WhereNotNull()
        {
            return this.Where(x =>
            {
                return (x != null) && !string.IsNullOrEmpty(x.ToString());
            });
        }

        /// <summary>
        /// Creates an instance of the target type and performs the CopyProperties action
        /// </summary>
        /// <returns>A new instance of : <see cref="T"/></returns>
        public T Create()
        {
            var result = Activator.CreateInstance<T>();

            this.CopyProperties(result);

            return result;
        }

        /// <summary>
        /// Updates the target instance while performing the CopyProperties action
        /// </summary>
        public void Update()
        {
            this.CopyProperties(this.target);
        }

        /// <summary>
        /// The CopyProperties action
        /// </summary>
        /// <param name="obj"><see cref="T"/></param>
        private void CopyProperties(T obj)
        {
            var outProperties = typeof(T).GetProperties().ToDictionary(x => new { x.Name });
            var inProperties = typeof(TSrc).GetProperties().ToDictionary(x => new { x.Name });

            foreach (var kvp in inProperties)
            {
                if (outProperties.ContainsKey(kvp.Key) && !excludedProperties.Contains(kvp.Value))
                {
                    var outType = outProperties[kvp.Key].PropertyType;
                    var inType = kvp.Value.PropertyType;

                    if (outType.IsAssignableFrom(inType))
                    {
                        var value = kvp.Value.GetValue(copySource);
                        var canWrite = outProperties[kvp.Key].CanWrite;

                        if (canWrite)
                        {
                            foreach (var query in queries)
                            {
                                if (!query.Invoke(value))
                                {
                                    canWrite = false;
                                    break;
                                }
                            }

                            if (canWrite)
                            {
                                outProperties[kvp.Key].SetValue(obj, value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Implicit assignment of action
        /// </summary>
        /// <param name="action"></param>
        public static implicit operator T(CopyPropertiesAction<T, TSrc> action)
        {
            return action.Create();
        }
    }
}
