using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.TestApp
{
    public class CacheAttribute : NETMetaCoderAttribute
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override void Init(bool async, Type containerType, Type returnType, string methodName,
            Type[] parameterTypes)
        {
            Console.WriteLine("in cache init");

            var pt = parameterTypes.Aggregate("", (acc, t) => $"{acc}, {t.Name}");
            Console.WriteLine(
                $"async={async}, returnType={returnType.Name}, methodName={methodName}, parameterTypes={pt}");
        }

        public override InterceptionResult Intercept(object[] arguments)
        {
            return InterceptionResult.NotIntercepted();
        }

        public override InterceptionResult<TValue> Intercept<TValue>(object[] arguments, ref TValue value)
        {
            return InterceptionResult<TValue>.NotIntercepted();
        }

        public override void HandleInterceptionResult(ref InterceptionResult interceptionResult)
        {
        }

        public override void HandleInterceptionResult<TValue>(ref TValue value,
            ref InterceptionResult<TValue> interceptionResult)
        {
        }
    }
}
