#pragma warning disable 1998
#pragma warning disable 4014

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedTypeParameter
// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#region

using NETMetaCoder;
using NETMetaCoder.TestApp;

#endregion

namespace Namespace1
{
    namespace Namespace1__Namespace1
    {
        public class Namespace1__Namespace1__Class1
        {
            [Cache]
            public async Task<IEnumerable<List<int>>> Foo()
            {
                return new []{new List<int>()};
            }
        }
    }

    [Foo]
    public class Namespace1__Class2 : TestBase, IFace
    {
        [NETMetaCoder.TestApp.Cache]
        [Foo]
        public char Namespace1__Class2__Method1()
        {
            return 'x';
        }

        [Foo]
        public int Namespace1__Class2__Method2(int i) => 5;

        // [Cache]
        public Task Namespace1__Class2__Method3() => Task.FromCanceled(CancellationToken.None);

        [Cache]
        public Task<int> Namespace1__Class2__Method4(int i) => Task.FromResult(i);

        // [Cache]
        public async Task Namespace1__Class2__Method5() => Task.FromCanceled(CancellationToken.None);

        [Cache]
        public async Task<int> Namespace1__Class2__Method6(int i) => i;

        [Logger]
        public async Task Namespace1__Class2__Method7() => Task.FromCanceled(CancellationToken.None);

        [Logger]
        public Task Namespace1__Class2__Method8() => Task.FromCanceled(CancellationToken.None);

        [Foo]
        [NETMetaCoder.TestApp.Cache]
        public bool Namespace1__Class2__Method3(Class1 leClass)
        {
            Console.WriteLine($"LeClass says: {leClass.LeInt}");

            return true;
        }

        [NETMetaCoder.TestApp.Cache]
        private static int Namespace1__Class2__Method4()
        {
            return 1;
        }

        class Namespace1__Class2__Class1
        {
            public int Namespace1__Class2__Class1__Method1(int i) => 8;

            class Namespace1__Class2__Class1__Class1
            {
                [NETMetaCoder.TestApp.Cache]
                public string Namespace1__Class2__Class1__Class1__Method1()
                {
                    return "abc";
                }
            }

            class Namespace1__Class2__Class1__Class2
            {
            }
        }

        class Namespace1__Class2__Class2
        {
            [NETMetaCoder.TestApp.Cache]
            public bool Namespace1__Class2__Class2__Method1()
            {
                return true;
            }
        }

        class Namespace1__Class2__Class3
        {
        }

        [NETMetaCoder.TestApp.Cache]
        int IFace.Kalua()
        {
            Console.Write("Drinking kalua...");

            return 2;
        }

        [NETMetaCoder.TestApp.Cache]
        public override int Wine()
        {
            Console.WriteLine("Drinking wine...");

            return base.Wine();
        }

        [NETMetaCoder.TestApp.Cache]
        public sealed override char Apple()
        {
            Console.WriteLine("Eating apples...");

            return base.Apple();
        }
    }

    namespace Namespace1__Namespace2
    {
        public static class Namespace1__Namespace2__Class1
        {
            [NETMetaCoder.TestApp.Cache]
            public static bool Namespace1__Namespace2__Class1__Method1()
            {
                return false;
            }

            public static int Namespace1__Namespace2__Class1__Method2(int i) => 7;

            class Namespace1__Namespace2__Class1__Class1
            {
                public int Namespace1__Namespace2__Class1__Class1__Method1(int i) => 8;
            }

            class Namespace1__Namespace2__Class1__Class2
            {
                [NETMetaCoder.TestApp.Cache]
                public string Namespace1__Namespace2__Class1__Class2__Method1()
                {
                    return "abc123";
                }
            }

            class Namespace1__Namespace2__Class1__Class3
            {
            }
        }

        public static class Namespace1__Namespace2__Class2
        {
            [NETMetaCoder.TestApp.Cache]
            public static int Namespace1__Namespace2__Class2__Method1()
            {
                return 50;
            }

            public static int Namespace1__Namespace2__Class2__Method2(int i) => 7;

            class Namespace1__Namespace2__Class2__Class1
            {
                public int Namespace1__Namespace2__Class2__Class1__Method1(int i) => 8;
            }

            class Namespace1__Namespace2__Class2__Class2
            {
                [NETMetaCoder.TestApp.Cache]
                public bool Namespace1__Namespace2__Class2__Class2__Method1()
                {
                    return false;
                }
            }

            class Namespace1__Namespace2__Class2__Class3
            {
                // εδώ κάτι θα γίνει κάποια στιγμή
            }
        }
    }
}

public class Class1
{
    public int LeInt { get; set; } = 5;

    [NETMetaCoder.TestApp.Cache]
    public char Class1__Method1()
    {
        return '1';
    }

    public int Class1__Method2(int i) => 6;

    public static class Class1__Class1
    {
        [NETMetaCoder.TestApp.Cache]
        public static string Class1__Class1__Method1()
        {
            return "qwerty";
        }

        public static int Class1__Class1__Method2(int i) => 7;

        class Class1__Class1__Class1
        {
            public int Class1__Class1__Class1_Method1(int i) => 8;
        }

        static class Class1__Class1__Class2
        {
            [NETMetaCoder.TestApp.Cache]
            public static char Class1__Class1__Class2__Method1()
            {
                return 'g';
            }
        }

        public class Class1__Class1__Class3
        {
            public struct StructInner<X>
            {
                public static void StructInner__Method1()
                {
                }

                // [NETMetaCoder.TestApp.Cache]
                // [Logger]
                public static IEnumerable<Z> StructInner__Method2<T, U, Z>(Task<U> x, Dictionary<T, U> y, ref int w)
                {
                    w *= 2;

                    return default;
                }

                [Logger]
                // [Cache]
                [Obsolete("StructInner__Method3", false)]
                public static IEnumerable<Z> StructInner__Method3<T, U, Z>(Task<U> x, Dictionary<T, U> y, ref int w,
                    out string z)
                {
                    w *= 3;

                    z = "out";

                    return default;
                }

                [Logger]
                [Obsolete("StructInner__Method4", true)]
                public static Z StructInner__Method4<T, U, Z>(Task<U> x, Dictionary<T, U> y, T t)
                {
                    return default;
                }
            }
        }
    }

    public struct StructOuter
    {
        public static async Task StructOuter__Method1()
        {
        }

        [LoggerAttribute]
        [Obsolete]
        public static void StructOuter__Method2()
        {
        }

        public static void StructOuter__Method3()
        {
        }

        [Cache]
        [Logger]
        [Obsolete("StructOuter__Method4")]
        public static async Task<int> StructOuter__Method4(int x)
        {
            return x;
        }
    }
}
