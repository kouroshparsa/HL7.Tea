using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace HL7.Tea.Core
{
    public static class FilterCompiler
    {
        private static string REDIS_CON_STR = Environment.GetEnvironmentVariable("REDIS_CON_STR");

        private static readonly Lazy<ConnectionMultiplexer> _redis =
                new Lazy<ConnectionMultiplexer>(() => REDIS_CON_STR==null ? null:
                    ConnectionMultiplexer.Connect(REDIS_CON_STR));

        private static IDatabase Database => _redis.Value?.GetDatabase();

        private static Expression BuildEq(Expression callExpr, object value)
        {
            var val = Expression.Constant(value?.ToString());
            return Expression.Equal(callExpr, val);
        }

        private static Expression BuildNe(Expression callExpr, object value)
        {
            var val = Expression.Constant(value?.ToString());
            return Expression.NotEqual(callExpr, val);
        }

        private static Expression BuildContains(Expression callExpr, object value)
        {
            var val = Expression.Constant(value?.ToString());

            var notNull = Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));

            var contains = Expression.Call(
                callExpr,
                typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                val
            );

            return Expression.AndAlso(notNull, contains);
        }

        private static Expression BuildIContains(Expression callExpr, object value)
        {
            var val = Expression.Constant(value?.ToString());

            var notNull = Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));

            var indexOf = Expression.Call(
                callExpr,
                typeof(string).GetMethod("IndexOf", new[] { typeof(string), typeof(StringComparison) }),
                val,
                Expression.Constant(StringComparison.OrdinalIgnoreCase)
            );

            var contains = Expression.GreaterThanOrEqual(indexOf, Expression.Constant(0));

            return Expression.AndAlso(notNull, contains);
        }

        private static Expression BuildStartsWith(Expression callExpr, object value)
        {
            var val = Expression.Constant(value?.ToString());

            var notNull = Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));

            var starts = Expression.Call(
                callExpr,
                typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                val
            );

            return Expression.AndAlso(notNull, starts);
        }

        private static Expression BuildEndsWith(Expression callExpr, object value)
        {
            var val = Expression.Constant(value?.ToString());

            var notNull = Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));

            var ends = Expression.Call(
                callExpr,
                typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                val
            );

            return Expression.AndAlso(notNull, ends);
        }

        private static Expression BuildRegex(Expression callExpr, object value)
        {
            var pattern = Expression.Constant(value?.ToString());

            var notNull = Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));

            var isMatchMethod = typeof(System.Text.RegularExpressions.Regex)
                .GetMethod("IsMatch", new[] { typeof(string), typeof(string) });

            var match = Expression.Call(isMatchMethod, callExpr, pattern);

            return Expression.AndAlso(notNull, match);
        }

        private static Expression BuildIn(Expression callExpr, object value)
        {
            if (value is not JsonElement json)
                throw new ArgumentException("IN expects JsonElement");

            if (json.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("IN operator requires array");

            var list = json.EnumerateArray()
                           .Select(x => x.GetString())
                           .ToList();

            var listExpr = Expression.Constant(list);

            // This is like calling GetMethod(string name, Type[] types) and you need to pass the list of parameter types
            // so that it knows which overload of Contains method it should call.
            var containsMethod = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });

            var notNull = Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));

            var contains = Expression.Call(listExpr, containsMethod, callExpr);

            return Expression.AndAlso(notNull, contains);
        }

        private static Expression BuildExists(Expression callExpr)
        {
            return Expression.NotEqual(callExpr, Expression.Constant(null, typeof(string)));
        }


        private static Expression BuildInCache(Expression callExpr, object value)
        {
            // callExpr is like msg.GetFieldOne("MSH-9.2")
            // and value is the table name in the "value" attribute of the spec.json
            var tableNameExpr = Expression.Constant(value?.ToString());
            var method = typeof(RedisCache).GetMethod(
                    nameof(RedisCache.IsInCache),
                    new[] { typeof(string), typeof(string) }
                );

            return Expression.Call(
                method,
                tableNameExpr,
                callExpr
            );
        }

        private static Expression BuildCondition(Condition cond, ParameterExpression param)
        {
            var field = Expression.Constant(cond.Field);
            var value = Expression.Constant(cond.Value); // this is the value from spec
            var method = typeof(HL7Message).GetMethod("GetFieldOne");
            var callExpr = Expression.Call(param, method, field); // this is the value extracted from HL7Message

			return cond.Operator switch
            {
                "eq" => BuildEq(callExpr, cond.Value),
                "ne" => BuildNe(callExpr, cond.Value),
                "contains" => BuildContains(callExpr, cond.Value),
                "icontains" => BuildIContains(callExpr, cond.Value),
                "starts_with" => BuildStartsWith(callExpr, cond.Value),
                "ends_with" => BuildEndsWith(callExpr, cond.Value),
                "regex" => BuildRegex(callExpr, cond.Value),
                "in" => BuildIn(callExpr, cond.Value),
                "exists" => BuildExists(callExpr),
                "in_cache" => BuildInCache(callExpr, cond.Value),
                _ => throw new NotSupportedException($"Operator {cond.Operator} not supported")
            };
        }

        public static Func<HL7Message, bool>BuildFilter(string spec)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var model = JsonSerializer.Deserialize<FilterModel>(spec, options);
            return Compile(model.Filters);
        }
        private static Func<HL7Message, bool> Compile(FilterNode node)
        {
            var param = Expression.Parameter(typeof(HL7Message), "msg");
            var body = BuildExpression(node, param);
            return Expression.Lambda<Func<HL7Message, bool>>(body, param).Compile();
        }

        private static Expression BuildExpression(FilterNode node, ParameterExpression param)
        {
            switch (node)
            {
                case Condition cond:
                    return BuildCondition(cond, param);

                case AndGroup and:
                    return and.And
                        .Select(n => BuildExpression(n, param))
                        .Aggregate(Expression.AndAlso);

                case OrGroup or:
                    return or.Or
                        .Select(n => BuildExpression(n, param))
                        .Aggregate(Expression.OrElse);

                case NotGroup not:
                    return Expression.Not(BuildExpression(not.Not, param));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
