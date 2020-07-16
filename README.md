## NETMetaCoder

A group of libraries along with some MSBuilt tasks for rewriting a project's
code files by wrapping method calls, through the use of attributes.

In summary, one can use attributes to mark methods for wrapping.

Then, this library will produce two new code files for each such method, one
with the original name and with minor changes such as marking a class as
partial, and another one (aka, a companion file), which contains an alternative
method implementation that wraps a call to the original method.

#### Usage

To wrap a method in a project, the following are the minimum amount of steps to
follow.

It is assumed that the user of NETMetaCoder is building a solution with two
projects.

One of them, named `Attributes`, contains the attribute implementations and
the other one, named `App`, contains the methods that must be wrapped by
the attribute implementations in the `Attributes` project. 

###### 1. Install `NETMetaCoder.Abstrations` into `Attributes`

The `Attributes` project needs the `NETMetaCoder.Abstractions` package in order
to be able to use the `NETMetaCoderAttribute` as a base class for its attribute
implementations.

###### 2. Install `NETMetaCoder.MSBuild` into `App`

The `App` project, which depends on the `Attributes` project, needs the
`NETMetaCoder.MSBuild` package for the MSBuild targets that it installs. As
such, when `App` is built, its code files will be scanned and if a target
attribute is found on a method, that method will be wrapped by generated code.

###### 3. Create a `NETMetaCoder.Index.json` file in `App`

`NETMetaCoder.MSBuild` searches for `NETMetaCoder.Index.json` starting from the
project's root directory and moving upwards until the root of the filesystem is
reached.

As such, in a solution in which there are multiple projects, the
`NETMetaCoder.Index.json` file can be place in the solution's root directory.

An exhaustive example of the format of the `NETMetaCoder.Index.json` file, is
as follows:

```json
{
  "attributes": [
    {
      "name": "Cache",
      "order": 1,
      "wrapper": "WithoutGenericParametersWrapper"
    },
    {
      "name": "Logger",
      "order": 2,
      "wrapper": "CommonWrapper"
    }
  ]
}
```

For all the acceptable values for the `wrapper` key, please, refer to
`SyntaxWrappersIndex.Wrappers`, in the `NETMetaCoder.SyntaxWrappers` library.

The above example describes the following:

1. A method to which the attribute named `Cache` has been applied in the `App`
  project, will be wrapped using the syntax fragments produced by the
  `WithoutGenericParametersWrapper` wrapper type, as configured in
  `SyntaxWrappersIndex.Wrappers`.

2. A method to which the attribute named `Logger` has been applied in the `App`
  project, will be wrapped using the syntax fragments produced by the
  `CommonWrapper` wrapper type, as configured in
  `SyntaxWrappersIndex.Wrappers`.

3. A method to which both of the above attributes have been applied in the
  `App` project, will first be wrapped by the syntax fragments produced for the
  `Cache` attribute and second, by the syntax fragments produced for the
  `Logger` attribute.
  
As such, a method of the following form:

```c#
[Cache]
public async Task<IEnumerable<List<int>>> Foo()
{
    return new []{new List<int>()};
}
```

will be changed into:

```c#
[Cache]
public async Task<IEnumerable<List<int>>> Foo__WrappedByCache()
{
    return new []{new List<int>()};
}
```

and the method that will be actually called by client code will have the
following form:

```c#
[NETMetaCoderMarker]
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public Task<IEnumerable<List<int>>> Foo()
{
    {
        Task<IEnumerable<List<int>>> __result = default(Task<IEnumerable<List<int>>>);
        var __attributeCache = Foo__PropertyForAttributeCache__CAB8111FD0B710A336C898E539090E34.Value;
        var __interceptionResultCache = __attributeCache.Intercept(new object[]{}, ref __result);
        if (!__interceptionResultCache.IsIntercepted)
        {
            try
            {
                __result = Foo__WrappedByCache();
            }
            catch (Exception exception)
            {
                if (!__attributeCache.HandleException(exception, ref __result, ref __interceptionResultCache))
                {
                    throw;
                }
            }
        }
        __attributeCache.HandleInterceptionResult(ref __result, ref __interceptionResultCache);
        return __result;
    }
}
```

The `NETMetaCoderMarkerAttribute` attribute enables runtime access to a
particular instance of a wrapper attribute.

The number of `NETMetaCoderMarkerAttribute` attributes that are applied to a
wrapped method is equal to the wrapper attributes that caused that method to be
wrapped.

#### Debugging

###### Visual Studio

To properly debug code in Visual Studio, the setting
`Require source files to exactly match the original version` must not be
selected in `Tools -> Options Debugging -> General`.

This is necessary because `NETMetaCoder` changes the original code file.

###### JetBrains Rider

When debugging in JetBrains Rider, if the `KeepNETMetaCoderOutput` property is
set to `true` in the `.csproj` file, then the breakpoint will be hit but the
IDE will open the corresponding file in the
`obj/NETMetaCoderRewrittenCodeSyntax` directory.

If the `KeepNETMetaCoderOutput` property is set to `false`, the breakpoint will
be hit but the error `Could not get symbols` will be reported.

In either case, the IDE fails to identify that the breakpoint should be hit in
the original code file.

As such, it is advisable to set the `KeepNETMetaCoderOutput` property to `true`
when debugging locally.

#### Libraries in the NETMetaCoder solution

###### NETMetaCoder

This is the core library which contains the logic for rewriting a code file.

It works on a file-by-file basis.

###### NETMetaCoder.Abstractions

This library acts as a common denominator and is meant to be used by projects
that depend on NETMetaCoder.

Most importantly, it contains the definition of `NETMetaCoderAttribute`, which
is meant to be used as the base class for all method attributes, which will be
targeted by a dependant project.

###### NETMetaCoder.MSBuild

This library contains the MSBuild tasks that get triggered, in the context of
a dependant project's build process.

It is mainly comprised by a generated `.targets` file
(`NETMetaCoder.MSBuild.targets`) and the `RewriteProjectSyntax` MSBuild task.

When building the dependant project, the `RewriteProjectSyntax` MSBuild task
scans code files and uses the `NETMetaCoder` library to rewrite their syntax,
if necessary.

This library, although it depends on all others, bundles its dependencies so
that it's easier to find them when triggered by MSBuild.

###### NETMetaCoder.SyntaxWrappers

This library encapsulates the several syntax wrapper types that are supported
by NETMetaCoder.

A syntax wrapper type, at a minimum, defines which `UsingDirectiveSyntax`s and
`SyntaxWrapper`s must be used to wrap a call to the original method.

A `SyntaxWrapper` is an object that contains a collection of two flavors of
`MethodSyntaxGenerator`s.

One flavor of `MethodSyntaxGenerator`s generates code that is placed before the
call to the original method and the other generates code that is placed after
it.

This collection could be described as:
```
{
    { Before: MethodSyntaxGenerator, After: MethodSyntaxGenerator },
    ...
}
```
and when combining these syntax wrappers, all `Before` `MethodSyntaxGenerator`s
generate syntax fragments that are combined and placed before the call to the
original method and all `After` `MethodSyntaxGenerator`s generate syntax
fragments that are combined and placed after the call to the original method
call.
