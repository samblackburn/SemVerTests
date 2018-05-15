# How should we do semantic versioning?
An illustration of the problems we encounter when not doing semantic versioining right

## Approach 1: Use 4-part DLL versions
 - If we version our .NET assemblies normally, with `major.minor.build.revision` versions, then nuget upgrades will tend to cause dependencies to be incompatible with each other: https://github.com/samblackburn/SemVerTests/blob/master/FourPartVersioningTests.cs
 - A possible solution to this is to use an assembly redirect: https://github.com/samblackburn/SemVerTests/blob/master/AssemblyResolveEventTests.cs

## Approach 2: Use Major.0.0.0 DLL versions
 - If we version our .NET assemblies with `major.0.0.0` then we get a different set of problems when two products are run in the same process: https://github.com/samblackburn/SemVerTests/blob/master/MajorVersionOnlyTests.cs
