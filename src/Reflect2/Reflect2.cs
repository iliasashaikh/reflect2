using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Trycatchthat
{
    public static class Reflect2
    {
        class MethodLookupInfo
        {
            public Type Type { get; set; }
            public string Name { get; set; }
            public List<Type> ArgTypes { get; set; }

            public class Comparer : IEqualityComparer<MethodLookupInfo>
            {
                public bool Equals(MethodLookupInfo x, MethodLookupInfo y)
                {
                    return x.Type == y.Type &&
                            x.Name == y.Name &&
                            ((x.ArgTypes == null && y.ArgTypes == null) || Enumerable.SequenceEqual<Type>(x.ArgTypes, y.ArgTypes));
                }

                public int GetHashCode(MethodLookupInfo lookupInfo)
                {
                    int argCount = lookupInfo.ArgTypes == null ? 0 : lookupInfo.ArgTypes.Count;
                    return 17 * lookupInfo.Type.GetHashCode() * lookupInfo.Name.GetHashCode() * (lookupInfo.ArgTypes.Count+1);
                }
            }

            public static MethodLookupInfo Make(string methodName, Type type, params object[] args)
            {
                return new MethodLookupInfo { Name = methodName, Type = type, ArgTypes = args.Select(a=>a.GetType()).ToList()};
            }

        }

        static Dictionary<MethodLookupInfo, Delegate> lookup = new Dictionary<MethodLookupInfo, Delegate>(new MethodLookupInfo.Comparer());
        public static TReturn Run<TSource, TReturn>(string methodName, TSource instance = default(TSource), object[] args = null)
        {
            var sourceType = typeof(TSource);

            var key = MethodLookupInfo.Make(methodName, sourceType, args);
            Delegate caller;

            if (lookup.TryGetValue(key, out caller))
                return Run<TReturn>(caller, args);

            var methodInfo = typeof(TSource)
                                    .GetMethod(methodName,
                                                System.Reflection.BindingFlags.Instance
                                                | System.Reflection.BindingFlags.Public
                                                | System.Reflection.BindingFlags.NonPublic,
                                                null,
                                                args == null ? null : args.Select(a => a.GetType()).ToArray(),
                                                null);
            var methodParams = methodInfo.GetParameters();
            // create an expression (arg0,arg1,arg2...)=>instance.Method((int)arg0,(int)arg1,(int)arg2..);
            var paramExpressionsDecl = args.Select(a => Expression.Parameter(typeof(object))).ToArray();
            Expression[] paramExpressionsBody = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                paramExpressionsBody[i] = Expression.Convert(paramExpressionsDecl[i], methodParams[i].ParameterType);
            }
            var bodyExpr = Expression.Call(Expression.Constant(instance), methodInfo, paramExpressionsBody);
            var lambdaExpr = Expression.Lambda(bodyExpr, paramExpressionsDecl);
            caller = lambdaExpr.Compile();
            lookup[key] = caller;

            return Run<TReturn>(caller, args);
            
        }

        private static TReturn Run<TReturn>(Delegate caller, object[] args)
        {
            var f2 = caller as Func<object, object, TReturn>;
            if (f2 != null)
              return f2(args[0], args[1]);

            throw new NotImplementedException();
        }
        private static TReturn Run<TReturn, TArg1, TArg2>(Delegate caller, TArg1 arg1, TArg2 arg2)
        {
            var f = caller as Func<TArg1, TArg2, TReturn>;
            if (f != null)
                return f(arg1, arg2);

            //switch (args.Length)
            //{
            //    case 0:
            //        var f = caller as Func<TReturn>;
            //        if (f != null)
            //            return f();
            //        break;

            //    case 1:
            //        var f1 = caller as Func<object, TReturn>;
            //        if (f1 != null)
            //            return f1(args[0]);
            //        break;

            //    case 2:
            //        var f2 = caller as Func<object, object, TReturn>;
            //        if (f2 != null)
            //            return f2(args[0], args[1]);
            //        break;
            //    case 3:
            //    case 4:
            //    case 5:
            //    case 6:
            //    default:
            //        break;
            //}

            throw new NotImplementedException();
        }

        public static void Action<TSource>(string methodName, TSource instance = default(TSource), params object[] args)
        {
            throw new NotImplementedException();
        }

        public static TProperty GetProperty<Tsource, TProperty>(string propertyName, Tsource instance= default(Tsource))
        {
            throw new NotImplementedException();
        }

        public static void SetProperty<Tsource, TProperty>(string propertyName, Tsource instance = default(Tsource), TProperty value = default(TProperty))
        {
            throw new NotImplementedException();
        }

        public static TSource Create<TSource>(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
