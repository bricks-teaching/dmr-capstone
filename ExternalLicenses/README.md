# LICENSES
All licenses for third-party assets or source code are stored here to respect their terms where applicable. When the modifications section is omitted, the asset in question was not modified by the DMR team.

* [Assets](#assets)
* [Source Code](#third-party-source-code)

----

# Assets
Graphics and audio assets are spread across the project.

1. [ATM model](#atm)
1. [Market stand model](#market-stand)
1. [Raygun model](#raygun)
1. [Robot enemy models](#robot-enemies)
1. [Shopkeeper NPC model](#shopkeeper-npc)
1. [Wooden sign model](#wooden-sign)

----
## ATM
[Original](https://www.thingiverse.com/thing:3809950)

Author: Page1208

License: [CC-BY 3.0](https://creativecommons.org/licenses/by/3.0/)

Files:
- `ATM1.fbx`

Modifications:
1. Added texture created by @zalfenior

----
## Market stand
[Original](https://www.turbosquid.com/FullPreview/Index.cfm/ID/978391)

Author: DoodleStudio

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

Files:
- `market-stand.fbx`

----
## Raygun
[Original](https://www.turbosquid.com/3d-models/free-blend-mode-gun-raygun/337099)

Author: knobknoster

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

Files:
- *TBD*

Modifications:
1. *TBD*

----
## Robot Enemies
[Original](https://opengameart.org/content/robot-enemy-pack)

License: [CC0 1.0](http://creativecommons.org/publicdomain/zero/1.0/)

Files:
- *TBD*

Modifications:
1. *TBD*

----
## Shopkeeper NPC
[Original](https://www.turbosquid.com/FullPreview/Index.cfm/ID/1284322)

Author: Vertici

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

Files:
- `Shopkeep.fbx`

----
## Wooden sign
[Original](https://www.turbosquid.com/FullPreview/Index.cfm/ID/1263514)

Author: Defelozedd94

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

Files:
- `Wooden_sign.fbx`

----


# Third-Party Source Code
The `fiscal-shock/Scripts/ThirdParty` directory contains all third-party source code not written or ported by the DMR team.

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

----
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
