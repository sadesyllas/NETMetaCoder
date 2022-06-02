using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.TestApp
{
    public class CacheAttribute : NETMetaCoderAttribute
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override void Init(MethodBase wrapperMethodBase, MethodInfo wrappedMethodInfo)
        {
            Console.WriteLine("in cache init");

            var guid1 = ((NETMetaCoderMarkerAttribute)wrapperMethodBase.GetCustomAttributes()
                .First(a => a is NETMetaCoderMarkerAttribute)).Id;

            var guid2 = ((NETMetaCoderMarkerAttribute)wrapperMethodBase.GetCustomAttributes()
                .First(a => a is NETMetaCoderMarkerAttribute)).Id;

            Debug.Assert(guid1 == guid2, "GUIDs do not match");

            var pt = string.Join(", ", wrappedMethodInfo.GetParameters().Select(p => p.ParameterType.Name));

            Console.WriteLine(
                $"guid={guid1}, async={wrappedMethodInfo.IsAsync()}, returnType={wrappedMethodInfo.ReturnType.Name}, " +
                $"methodName={wrappedMethodInfo.Name}, parameterTypes={pt}");
        }

        public override InterceptionResult Intercept(object[] arguments, Action _)
        {
            return InterceptionResult.NotIntercepted();
        }

        public override InterceptionResult<TValue> Intercept<TValue>(object[] arguments, ref TValue value,
            Func<TValue> _)
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
