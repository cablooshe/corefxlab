using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppDomainComparison
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello!");
            var summary = BenchmarkRunner.Run<AppDomainTesting>();
            Console.ReadLine();
        }
    }



    public interface ITest
    {
        string PrintContext();
        int DoThing2(int a, List<string> list);
        int DoThing3(int a, Test2 t);
        Test2 ReturnUserType();
        int SimpleMethod();
        string CallUsingMultipleParameters(int a, int b, string s, Test2 t, int c, int x, int y, int[] z, Test2 tt);

    }

    public interface IGeneric<T>
    {
        string PrintContext();
        int DoThing2(int a, List<string> list);
        int DoThing3(int a, Test2 t);
        string DoThing4(T t);
        string GenericMethodTest<I>();
    }

    public class Test2 : MarshalByRefObject
    {
        public int test;
        public Test2()
        {
            test = 5;
        }
        public Test2(int start)
        {
            test = start;
        }
        public void DoThingy()
        {
            test++;
        }
    }
    public class GenericClass<T> : MarshalByRefObject, IGeneric<T>
    {
        private readonly string _instance = "testString";
        private T _instance2;
        public GenericClass()
        {
        }
        public GenericClass(T t)
        {
            _instance2 = t;
        }
        public string PrintContext()
        {
            var a = Assembly.GetExecutingAssembly();
            return AppDomain.CurrentDomain.ToString();
        }
        public string GenericMethodTest<I>()
        {
            return typeof(I).ToString();
        }
        public int DoThing2(int a, List<string> list)
        {
            return _instance.Length;
        }
        public int DoThing3(int a, Test2 t)
        {
            t.DoThingy();
            return 6;
        }
        public string DoThing4(T tester)
        {
            return tester.ToString();
        }
    }

    public class Test : MarshalByRefObject, ITest
    {

        public Test()
        {

        }

        public string CallUsingMultipleParameters(int a, int b, string s, Test2 t, int c, int x, int y, int[] z, Test2 tt)
        {
            return (a + b + (c * x * y)).ToString() + s + t.ToString() + tt.ToString() + z.ToString();
        }
        public string PrintContext()
        {
            var a = Assembly.GetExecutingAssembly();
            return AppDomain.CurrentDomain.ToString();
        }
        public int DoThing2(int a, List<string> list)
        {
            Console.WriteLine(a);

            return a + list[0].Length;
        }
        public int DoThing3(int a, Test2 t)
        {
            t.DoThingy();
            return 5;
        }

        public Test2 ReturnUserType()
        {
            return new Test2();
        }
        public int SimpleMethod()
        {
            return 3;
        }
    }
    public class AppDomainTesting
    {
        private readonly AppDomain domain;// = AppDomain.CreateDomain("Benchmark Domain");
        //private string assemblyString = typeof(Test).Assembly.FullName;
        private readonly ITest testObject;// = (ITest)domain.CreateInstanceAndUnwrap(typeof(Test).Assembly.FullName, "Test");

        private readonly ITest controlObject;// = new Test();
        private readonly IGeneric<Test2> genericObject;// = (IGeneric<Test2>) domain.CreateInstanceAndUnwrap(typeof(GenericClass<Test2>).Assembly.FullName, "GenericClass");// = ProxyBuilder<IGeneric<Test2>>.CreateGenericInstanceAndUnwrap(alc, Assembly.GetExecutingAssembly().CodeBase.Substring(8), "GenericClass", new Type[] { typeof(Test2) });
        private readonly IGeneric<Test2> genericControl;// = new GenericClass<Test2>();
        private readonly Type tt;
        private Test2 userInput;

        //[GlobalSetup]
        public AppDomainTesting()
        {
            userInput = new Test2();
            domain = AppDomain.CreateDomain("Benchmark Domain");
            Console.WriteLine(typeof(Test).Assembly.FullName);
            foreach(Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine(Assembly.GetExecutingAssembly().GetTypes());
            testObject = (Test)domain.CreateInstanceAndUnwrap(typeof(Test).Assembly.FullName, "AppDomainComparison.Test");            //genericObject = (IGeneric<Test2>)domain.CreateInstanceAndUnwrap(typeof(Test).Assembly.FullName, "GenericClass");
            //genericObject = (GenericClass<Test2>) domain.CreateInstanceAndUnwrap(typeof(GenericClass<Test2>).Assembly.FullName, "AppDomainComparison.GenericClass");// = ProxyBuilder<IGeneric<Test2>>.CreateGenericInstanceAndUnwrap(alc, Assembly.GetExecutingAssembly().CodeBase.Substring(8), "GenericClass", new Type[] { typeof(Test2) });
            controlObject = new Test();
            genericControl = new GenericClass<Test2>();
            tt = typeof(GenericClass<Test2>);
            genericObject = (GenericClass<Test2>)domain.CreateInstanceAndUnwrap(tt.Assembly.FullName, tt.FullName);
        }
    [Benchmark]
        public object CreateProxyObject()
        {
            // alc = new AssemblyLoadContext("BenchmarkContext", isCollectible: true);

            return (Test)domain.CreateInstanceAndUnwrap(typeof(Test).Assembly.FullName, "AppDomainComparison.Test");
        }
        [Benchmark]
        public object CreateControlObject()
        {
            return new Test();
        }
        [Benchmark]
        public object CallSimpleMethodThroughProxy()
        {
            return testObject.SimpleMethod();
        }
        [Benchmark]
        public object CallSimpleMethodControl()
        {
            return controlObject.SimpleMethod();
        }
        [Benchmark]
        public object CreateGenericProxy()
        {
            return (GenericClass<Test2>)domain.CreateInstanceAndUnwrap(tt.Assembly.FullName, tt.FullName);
        }
        [Benchmark]
        public object CreateGenericControl()
        {
            return new GenericClass<Test2>();
        }
        [Benchmark]
        public object CallSimpleMethodGeneric()
        {
            return genericObject.PrintContext();
        }
        [Benchmark]
        public object CallSimpleMethodGenericControl()
        {
            return genericControl.PrintContext();
        }
        [Benchmark]
        public object UserTypeParameters()
        {
            return testObject.DoThing3(3, new Test2());
        }
        [Benchmark]
        public object UserTypeParametersControl()
        {
            return controlObject.DoThing3(3, new Test2());
        }
        [Benchmark]
        public object UserTypeParameters2()
        {
            return testObject.DoThing3(3, userInput);
        }
        [Benchmark]
        public object UserTypeParametersControl2()
        {
            return controlObject.DoThing3(3, userInput);
        }
        [Benchmark]
        public object SerializeManyParameters()
        {
            return testObject.CallUsingMultipleParameters(1, 2, "3", new Test2(), 44, 1, 3, new int[] { 3, 4, 5 }, new Test2());
        }

        [Benchmark]
        public object SerializeManyParametersControl()
        {
            return controlObject.CallUsingMultipleParameters(1, 2, "3", new Test2(), 44, 1, 3, new int[] { 3, 4, 5 }, new Test2());
        }
    }
}
