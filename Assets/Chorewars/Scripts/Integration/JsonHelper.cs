using System;
using System.Collections.Generic;

namespace Chorewars.Integration
{
    public static class JsonHelper
    {
        public static List<T> FromJson<T>(string json)
        {
            var wrapper = JsonWrapper<T>.Create(json);
            return wrapper?.items;
        }

        [Serializable]
        private class JsonWrapper<T>
        {
            public List<T> items;

            public static JsonWrapper<T> Create(string json)
            {
                var wrapped = $"{{\"items\":{json}}}";
                return UnityEngine.JsonUtility.FromJson<JsonWrapper<T>>(wrapped);
            }
        }
    }
}
