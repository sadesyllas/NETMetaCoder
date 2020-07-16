using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.TestApp
{
    public class LoggerAttribute : NETMetaCoderAttribute
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override void Init(MethodBase wrapperMethodBase, MethodInfo wrappedMethodInfo)
        {
            Console.WriteLine("in cache init");

            var pt = wrappedMethodInfo.GetParameters().Aggregate("", (acc, t) => $"{acc}, {t.ParameterType.Name}");
            Console.WriteLine(
                $"async={wrappedMethodInfo.IsAsync()}, returnType={wrappedMethodInfo.ReturnType.Name}, " +
                $"methodName={wrappedMethodInfo.Name}, parameterTypes={pt}");
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
