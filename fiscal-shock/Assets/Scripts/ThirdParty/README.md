# Third-Party Source Code
This directory contains all third-party source code not written or ported by the DMR team.

* [MersenneTwister](#mersennetwister)
* [Delaunator](#delaunator)

----

## MersenneTwister
[Original](https://www.codeproject.com/Articles/164087/Random-Number-Generation)

Author: logicchild

License: [CPOL](http://www.codeproject.com/info/cpol10.aspx)

Files:
- `MersenneTwister.cs`

Modifications:
1. Changed formatting to be more readable (whitespace and some parentheses)
1. Added comment with source and license information
1. Wrapped code in the namespace `ThirdParty`
1. Added `#pragma warning disable` directive to prevent Roslyn code analysis

## Delaunator
[Original](https://github.com/wolktocs/delaunator-csharp)

Author: wolktocs

License: [ISC](https://github.com/wolktocs/delaunator-csharp/blob/master/LICENSE)

Files:
- `Delaunator.cs`
- `ListExtensions.cs`

Modifications:
1. Included only the `Delaunator` source code; no project files, tests, or benchmarking files were added
1. Wrapped code in the namespace `ThirdParty`
1. Added `#pragma warning disable` directive to prevent Roslyn code analysis

