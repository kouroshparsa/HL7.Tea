using StackExchange.Redis;
using System;
using System.Linq;

namespace HL7.Tea.Core
{
    public static class RedisCache
    {
        private static string REDIS_CON_STR = Environment.GetEnvironmentVariable("REDIS_CON_STR");

        private static readonly Lazy<ConnectionMultiplexer> _redis =
        new Lazy<ConnectionMultiplexer>(() => REDIS_CON_STR == null ? null :
            ConnectionMultiplexer.Connect(REDIS_CON_STR));

        public static IDatabase? TestDatabase;

        private static IDatabase? Database =>
            TestDatabase ?? _redis.Value?.GetDatabase();


        public static bool IsInCache(string tableName, string key)
        {
            if (Database == null)
                throw new Exception("You must set the 'REDIS_CON_STR' environment variable to feth from cache.");

            var values = Database.StringGet(tableName);
            if (values.IsNullOrEmpty)
                return false;

            return values.ToString().Split(',').Contains(key);
        }

    }
}
