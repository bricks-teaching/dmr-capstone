# LICENSES
All licenses for third-party assets or source code are stored here to respect their terms where applicable. When the modifications section is omitted, the asset in question was not modified by the DMR team, or the license does not require such tracking.

* [Graphics](#graphics)
* [Sound](#sound)
* [Source Code](#third-party-source-code)

----

# Graphics
Graphics assets are spread across the project.

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

Modifications:
1. Added texture created by @zalfenior

----
## Icons
RAM icon is free for non-commercial use from [PNGWave](https://www.pngwave.com/png-clip-art-tpaxv).

Ram icon made by [Vitality Gorbachev](https://www.flaticon.com/authors/vitaly-gorbachev) from [Flaticon](https://www.flaticon.com/).

----
## Market stand
[Original](https://www.turbosquid.com/FullPreview/Index.cfm/ID/978391)

Author: DoodleStudio

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

----
## Player
[Original](https://shop.bitgem3d.com/products/npc-character-low-poly-3d-proto-series)

Author: Bitgem

License: [Bitgem Royalty Free License](https://shop.bitgem3d.com/pages/terms-of-use-license)

----
## Raygun
[Original](https://www.turbosquid.com/3d-models/free-blend-mode-gun-raygun/337099)

Author: knobknoster

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

----
## Robot Enemies
[Original](https://opengameart.org/content/robot-enemy-pack)

Author: Teh_Bucket

License: [CC0 1.0](http://creativecommons.org/publicdomain/zero/1.0/)

----
## Shopkeeper NPC
[Original](https://www.turbosquid.com/FullPreview/Index.cfm/ID/1284322)

Author: Vertici

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

----
## Wooden sign
[Original](https://www.turbosquid.com/FullPreview/Index.cfm/ID/1263514)

Author: Defelozedd94

License: [TurboSquid Royalty Free License](https://blog.turbosquid.com/royalty-free-license/#Royalty-Free-License)

----

# Sound
Audio assets are spread across the project.

1. [Bullet firing and impact sounds](#bullet-firing-and-impact-sounds)
1. [Cash payment failure](#cash-payment-failure-sound)
1. [Cash payment succes](#cash-payment-success-sound)
1. [Dungeon and hub themes](#dungeon-and-hub-themes)
1. [Main menu and win themes](#main-menu-and-win-themes)
1. [Pause and lose themes](#pause-and-lose-themes)

----

## Bullet firing and impact sounds
*TBD*

----

## Cash payment failure sound
[Original](https://freesound.org/people/Jacco18/sounds/419023/)

Author: Jacco18

License: [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)

----

## Cash payment success sound
[Original](https://freesound.org/people/kiddpark/sounds/201159/)

Author: kiddpark

License: [CC-BY 3.0](https://creativecommons.org/licenses/by/3.0/)

----
## Dungeon and hub themes
[Original](https://www.gamedevmarket.net/asset/ultimate-free-music-bundle-5353/)

Author: Marma

License: [GDM Pro Licence](https://www.gamedevmarket.net/terms-conditions/#pro-licence)

----

## Main menu and win themes
[Original](https://www.gamedevmarket.net/asset/the-nihilore-collection-5366/)

Author: Nihilore

License: [GDM Pro Licence](https://www.gamedevmarket.net/terms-conditions/#pro-licence)

----

## Pause and lose themes
[Original](https://www.gamedevmarket.net/asset/royalty-free-music-pack/)

Author: Tim Beek

License: [GDM Pro Licence](https://www.gamedevmarket.net/terms-conditions/#pro-licence)

----

# Third-Party Source Code
The `fiscal-shock/Scripts/ThirdParty` directory contains all third-party source code not written or ported by the DMR team.

1. [MersenneTwister](#mersennetwister)
1. [Delaunator](#delaunator)

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
