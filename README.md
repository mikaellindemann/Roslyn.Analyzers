# Lindemann.Analyzers
[![Nuget](https://img.shields.io/nuget/v/Lindemann.Analyzers)](https://www.nuget.org/packages/Lindemann.Analyzers)

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/mikaellindemann/lindemann/4/master)
![Azure DevOps tests (branch)](https://img.shields.io/azure-devops/tests/mikaellindemann/lindemann/4/master)
![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/mikaellindemann/lindemann/4/master)

A small number of C# analyzers I didn't find in FxCopAnalyzers and Roslynator.

Currently has the following analyzers and code fixes:

* Remove redundant array creation expressions in params parameters ([MD0001](docs/analyzers/MD0001.md) and [MD0002](docs/analyzers/MD0002.md))
* Use `Contains` or `ContainsKey` in place of `TryGetValue` when discarding the value. ([MD0010](docs/analyzers/MD0010) and [MD0011](docs/analyzers/MD0011.md))

## Contribution
If anyone at all would like to contribute to the analyzers, please don't hesitate to open issues and pull requests.

As these analyzers are my first take on static code analysis, there might be mistakes, or just better ways of doing analysis as well as code fixes.

If at some point it makes sense to not have this project under my personal account, lets make a shared effort to move the code and change the name of the repository as well as the package.
