#pragma warning disable 162

// ReSharper disable HeuristicUnreachableCode

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NETMetaCoder.Abstractions;
using NETMetaCoder.MSBuild;
using NETMetaCoder.SyntaxWrappers;

namespace NETMetaCoder.TestApp
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            const bool compile = false;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (compile)
            {
                var projectRootPath = Directory.GetParent(Environment.CurrentDirectory).Parent!.Parent!.FullName;

                var codeFilePaths = Directory.GetFiles(projectRootPath, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("/obj/")).ToArray();

                var testCodeFilePaths = codeFilePaths.Where(codeFilePath => codeFilePath.EndsWith("TestClass.cs"))
                    .ToArray();

                var outputBasePath = Path.Combine(projectRootPath, "obj");

                var wrappers = AttributesIndexReader.Read(projectRootPath).Select(attributeDescriptor =>
                    {
                        var wrapperType = SyntaxWrappersIndex.WrapperTypes[attributeDescriptor.WrapperType];

                        return (attributeDescriptor, (
                            wrapperType.Usings,
                            wrapperType.PropertySyntaxGenerator,
                            wrapperType.StatementWrappers));
                    })
                    .ToImmutableDictionary(
                        kv =>
                        {
                            var (a, _) = kv;

                            return a;
                        },
                        kv =>
                        {
                            var (_, b) = kv;

                            return b;
                        });

                if (!wrappers.Any())
                {
                    throw new NETMetaCoderException(
                        "[NETMetaCoder] No attributes found to wrap. Consider removing the reference to NETMetaCoder.");
                }

                var codeWrapTransformationOptions = new CodeWrapTransformationOptions(
                    projectRootPath,
                    outputBasePath,
                    "NETMetaCoderRewrittenCodeSyntax",
                    wrappers);

                var codeTransformer = new CodeTransformer(codeWrapTransformationOptions);

                foreach (var codeFilePath in testCodeFilePaths)
                {
                    var _ = codeTransformer.Wrap(codeFilePath);
                }
            }
            else
            {
                var x = new Namespace1.Namespace1__Class2();
                x.Namespace1__Class2__Method3(new Class1());

                var y = (IFace) x;
                y.Kalua();

                int i = 5;

                Class1.Class1__Class1.Class1__Class1__Class3.StructInner<int>.StructInner__Method2<int, string, double>(
                    Task.FromResult("generics"), null, ref i);

// #pragma warning disable 618
//                 Console.WriteLine($"result = {Class1.StructOuter.StructOuter__Method4(5).GetAwaiter().GetResult()}");
// #pragma warning restore 618

                Task.Run(() =>
                {
                    Console.WriteLine(
#pragma warning disable 618
                        $"result = {Class1.StructOuter.StructOuter__Method4(5).GetAwaiter().GetResult()}");
#pragma warning restore 618
                }).Wait();

                // var z = new Namespace1.Namespace1__Class2();
                // z.Namespace1__Class2__Method3(new Class1());
                //
                // var w = (IFace) z;
                // w.Kalua();
            }
        }
    }
}
