using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using Trycatchthat;
using System.Linq.Expressions;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

namespace Trycatchthat.Tests
{

    public class Arithematic
    {
        // instance methods
        public int Add(int a, int b) => a + b;
        public int Subract(int a, int b) => a * b;
        
        //static methods
        public static int Multiply(int a, int b) => a * b;
        public static int Multiply(int a, int b, int c) => a * b * c;

        //actions
        public int Result { get; set; }

        public void AddToResult(int a) => Result += a;
    }

    [TestFixture]
    public class Reflect2Tests
    {

        public static void Main()
        {
            BenchmarkRunner.Run<Reflect2Tests>();
        }

        [Test]
        public void TestExpression()
        {
            //(a,b)=>ari.Add(a,b)
            var mi = typeof(Arithematic).GetMethod("Add");
            var paramExp = new ParameterExpression[2] { Expression.Parameter(typeof(int)), Expression.Parameter(typeof(int)) };
            var body = MethodCallExpression.Call(Expression.Constant(new Arithematic()), mi, paramExp);
            var lambda = Expression.Lambda<Func<int,int,int>>(
                    body,paramExp
                );
            var f = lambda.Compile();
            Assert.That(f(1, 3), Is.EqualTo(4));
        }

        [Test]
        public void TestExpression2()
        {
            //(a,b)=>ari.Add((int)a,(int)b)
            var mi = typeof(Arithematic).GetMethod("Add");
            var paramExp1 = new ParameterExpression[2] { Expression.Parameter(typeof(object)), Expression.Parameter(typeof(object)) };
            //var paramExp2 = new Expression[2] { Expression.Convert(Expression.Parameter(typeof(object)),typeof(int)),
            //                                    Expression.Convert(Expression.Parameter(typeof(object)),typeof(int))
            //                                  };

            var x = Expression.Convert(paramExp1[0], typeof(int));
            var y = Expression.Convert(paramExp1[1], typeof(int));

            var body = MethodCallExpression.Call(Expression.Constant(new Arithematic()), mi, x, y);
            var lambda = Expression.Lambda<Func<object, object, int>>(body, paramExp1);
            var f = lambda.Compile();
            Assert.That(f(1, 3), Is.EqualTo(4));
        }


        [Test]
        [Benchmark]
        public void TestInstanceMethod()
        {
            var sut = new Arithematic();
            var r = Reflect2.Run<Arithematic,int>("Add", sut, 1, 2);
            //var r2 = Reflect2.Run<Arithematic,int>("Add", sut, 1, 2);
        }

        [Test]
        [Benchmark]
        public void TestInstanceMethodReflection()
        {
            var sut = new Arithematic();
            typeof(Arithematic).GetMethod("Add").Invoke(sut, new object[] { 1, 2 });
            //typeof(Arithematic).GetMethod("Add").Invoke(sut, new object[] { 1, 2 });
        }

        [Test]
        public void TestStaticMethod()
        {

        }

        [Test]
        public void TestInstanceAction()
        {

        }

        [Test]
        public void TestStaticAction()
        {

        }

        [Test]
        public void TestInstancePropertySet()
        {

        }

        [Test]
        public void TestInstancePropertyGet()
        {

        }

        [Test]
        public void TestStaticPropertySet()
        {

        }

        [Test]
        public void TestStaticPropertyGet()
        {

        }



    }
}
