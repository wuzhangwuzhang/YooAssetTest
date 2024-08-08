using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class BuildContext
    {
        private readonly Dictionary<Type, IContextObject> _contextObjects = new();

        /// <summary>
        ///     清空所有情景对象
        /// </summary>
        public void ClearAllContext()
        {
            _contextObjects.Clear();
        }

        /// <summary>
        ///     设置情景对象
        /// </summary>
        public void SetContextObject(IContextObject contextObject)
        {
            if (contextObject == null)
                throw new ArgumentNullException("contextObject");

            var type = contextObject.GetType();
            if (_contextObjects.ContainsKey(type))
                throw new Exception($"Context object {type} is already existed.");

            _contextObjects.Add(type, contextObject);
        }

        /// <summary>
        ///     获取情景对象
        /// </summary>
        public T GetContextObject<T>() where T : IContextObject
        {
            var type = typeof(T);
            if (_contextObjects.TryGetValue(type, out var contextObject))
                return (T)contextObject;
            throw new Exception($"Not found context object : {type}");
        }
    }
}