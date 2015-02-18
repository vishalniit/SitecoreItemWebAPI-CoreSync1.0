using Mindtree.ItemWebApi.Pipelines.Configuration;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Mindtree.ItemWebApi.Pipelines.Common
{
    public enum APICachePriority
    {
        Default,
        NotRemovable
    }

    public class CoreSyncCache : MemoryCache
    {
        public CoreSyncCache() : base("defaultCustomCache") { }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy, item.RegionName);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            base.Set(CreateKeyWithRegion(key, regionName), value, policy);
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            if (!key.Contains("region:"))
                key = CreateKeyWithRegion(key, regionName);
            CacheItem temporary = base.GetCacheItem(key);
            return new CacheItem(key, temporary.Value, regionName);
        }

        public override object Get(string key, string regionName = null)
        {
            if (!key.Contains("region:"))
                key = CreateKeyWithRegion(key, regionName);
            return base.Get(key);
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                return (base.DefaultCacheCapabilities | System.Runtime.Caching.DefaultCacheCapabilities.CacheRegions);
            }
        }

        public override bool Contains(string key, string regionName = null)
        {
            return base.Contains(CreateKeyWithRegion(key, regionName));
        }

        private string CreateKeyWithRegion(string key, string region)
        {
            return "region:" + (region == null ? "null_region" : region) + ";key=" + key;
        }
    }

    public static class APICache
    {
        // Gets a reference to the default MemoryCache instance. 
        //private static ObjectCache cache = MemoryCache.Default;
        private static ObjectCache cache = new CoreSyncCache();
        private static CacheItemPolicy policy = null;
        private static CacheEntryRemovedCallback callback = null;
        public static string RegionName { get { return "CoreSync"; } }
        public static long CacheCount
        {
            get
            {
                long count = 0;
                if (cache != null)
                    count = cache.GetCount();
                return count;
            }
        }

        /// <summary>
        /// Function add the object to .NET Memory cache
        /// </summary>
        /// <param name="CacheKeyName">key for cache item</param>
        /// <param name="CacheItem">actual object to cache</param>
        /// <param name="APICacheItemPriority">Cache Priority</param>
        public static void AddToAPICache(String CacheKeyName, Object CacheItem,
            APICachePriority APICacheItemPriority, string regionName = null)
        {
            if (CacheItem != null && CacheKeyName != null && CacheKeyName.Length > 0)
            {
                callback = new CacheEntryRemovedCallback(APICachedItemRemovedCallback);
                policy = new CacheItemPolicy();
                policy.Priority = (APICacheItemPriority == APICachePriority.Default) ?
                        CacheItemPriority.Default : CacheItemPriority.NotRemovable;
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(Settings.CacheExpirationTime);
                policy.RemovedCallback = callback;
                //policy.ChangeMonitors.Add(new HostFileChangeMonitor(FilePath));

                // Add inside cache 
                cache.Set(CacheKeyName, CacheItem, policy, regionName);
            }
        }

        /// <summary>
        /// Check if the cache object exist with given key
        /// </summary>
        /// <param name="CacheKeyName"></param>
        /// <returns>bool flag which tell true or false</returns>
        public static bool isCacheExist(String CacheKeyName, string regionName = null)
        {
            bool result = false;
            if (CacheKeyName != null && CacheKeyName.Length > 0)
            {
                result = cache.Contains(CacheKeyName, regionName);
            }
            return result;
        }

        /// <summary>
        /// Get the Cache Item based on type you cast at time of calling
        /// </summary>
        /// <typeparam name="T">Specify the Type which you want from the cache object</typeparam>
        /// <param name="CacheKeyName">string cache key to find the object</param>
        /// <returns>Actual Object retrived from cache</returns>
        public static T GetAPICachedItem<T>(String CacheKeyName, string regionName = null)
        {
            //             
            T response = default(T);
            if (CacheKeyName != null && CacheKeyName.Length > 0)
            {
                response = (T)cache.Get(CacheKeyName, regionName);
            }
            return response;
        }

        /// <summary>
        /// Remove the object from cache
        /// </summary>
        /// <param name="CacheKeyName"></param>
        public static void RemoveAPICachedItem(String CacheKeyName, string regionName = null)
        {
            // 
            if (cache.Contains(CacheKeyName, regionName))
            {
                cache.Remove(CacheKeyName, regionName);
            }
        }

        private static void APICachedItemRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            // Log these values from arguments list             
            String strLog = String.Concat("Reason: ", arguments.RemovedReason.ToString(), "| Key-Name: ", arguments.CacheItem.Key, " | Value-Object: ", arguments.CacheItem.Value.ToString());
            Log.Audit(strLog, typeof(APICache));
        }

        /// <summary>
        /// Functions clear all cache at ones
        /// </summary>
        public static void ClearAllCache()
        {
            if (cache != null && CacheCount > 0)
            {
                List<string> keys = new List<string>();
                foreach (var key in cache)
                {
                    keys.Add(key.Key);
                }
                foreach (var key in keys)
                {
                    try
                    {
                        cache.Remove(key);
                    }
                    catch { }
                }
            }
        }

        public static List<string> GetKeys()
        {
            List<string> value = null;
            if (cache != null && CacheCount > 0)
            {
                value = cache.Select(key => key.Key).ToList();
            }
            return value;
        }
    }
}
