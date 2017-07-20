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
        class MethodLookupInfo : IEquatable<MethodLookupInfo>
        {
            public Type Type { get; set; }
            public string Name { get; set; }
            public List<Type> ArgTypes { get; set; }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public bool Equals(MethodLookupInfo other)
            {
                return this.Type == other.Type &&
                           this.Name == other.Name &&
                           ArgsEqual(this.ArgTypes, other.ArgTypes);
            }

            public override int GetHashCode()
            {
                int argCount = this.ArgTypes == null ? 0 : this.ArgTypes.Count;
                return 17 * this.Type.GetHashCode() * this.Name.GetHashCode() * (this.ArgTypes.Count + 1);
            }


            public static MethodLookupInfo Make(string methodName, Type type, params object[] args)
            {
                return new MethodLookupInfo { Name = methodName, Type = type, ArgTypes = args.Select(a=>a.GetType()).ToList()};
            }

            private bool ArgsEqual(List<Type> a, List<Type> b)
            {
                if ((a == null && b != null) || (a != null && b == null) || (a.Count != b.Count))
                    return false;

                for (int i = 0; i < a.Count; i++)
                {
                    if (a[i] != b[i])
                        return false;
                }
                return true;
            }

        }

        //static Dictionary<MethodLookupInfo, Delegate> lookup = new Dictionary<MethodLookupInfo, Delegate>(new MethodLookupInfo.Comparer());
        static Dictionary<MethodLookupInfo, Delegate> lookup = new Dictionary<MethodLookupInfo, Delegate>();
        static Dictionary<MethodLookupInfo, Delegate> lookup2 = new Dictionary<MethodLookupInfo, Delegate>();
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

        public static TReturn Run<TSource, TReturn, TArg1, TArg2>(string methodName, TSource instance , TArg1 arg1, TArg2 arg2)
        {
            var sourceType = typeof(TSource);

            var key = MethodLookupInfo.Make(methodName, sourceType, arg1, arg2);
            Delegate caller;

            if (lookup2.TryGetValue(key, out caller))
                return ((Func<TArg1, TArg2, TReturn>)caller)(arg1, arg2);
            var args = new object[] { arg1, arg2 };
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

            var param1 = Expression.Parameter(typeof(TArg1));
            var param2 = Expression.Parameter(typeof(TArg2));

            //Expression[] paramExpressionsBody = new Expression[args.Length];
            //for (int i = 0; i < args.Length; i++)
            //{
            //    paramExpressionsBody[i] = Expression.Convert(paramExpressionsDecl[i], methodParams[i].ParameterType);
            //}
            var bodyExpr = Expression.Call(Expression.Constant(instance), methodInfo, param1, param2);
            var lambdaExpr = Expression.Lambda<Func<TArg1,TArg2,TReturn>>(bodyExpr, param1, param2);
            caller = lambdaExpr.Compile();
            lookup2[key] = caller;

            return ((Func<TArg1, TArg2, TReturn>)caller)(arg1, arg2);

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
