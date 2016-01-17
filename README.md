# Ba2Tools

Provides managed access to BA2 archives.

[Changelog](CHANGELOG.md).

Current features:
* Extract files from general archives, including packed files.
* Extract files from texture archives.
* Supports extracting to Stream, not only in file system.
* List files in archive

Upcoming features:
* Write BA2 archives
* Faster extraction speed

Sample:
```c#
using (var archive = BA2Loader.Load("D:\\Games\\Steam\\steamapps\\Fallout 4\\Data\\Fallout 4 - Interface.ba2"))
{
	string[] filesInArchive = archive.ListFiles();

	if (filesInArchive.Length > 1)
	{
		archive.Extract(filesInArchive[0], "D:\\Modding\\Fallout 4 Data\\Interface");
	}
}
```

[Show me more samples](Samples/).

## Speed

Extraction speed test on Fallout4 - Textures1.ba2 (extract all files task)

| Language | Tool              | Extraction time | Notes
| -------- | ----------------- | --------------- | -----
| C++      | ba2extract v0003  | 0 min 32 secs   |
| C++      | B.A.E. v0.0.4     | 1 min 9 secs    | rough GUI measurement
| C#       | Ba2Tools v0.2.0   | 1 min 35 secs   |

I want to mention that disk speed is huge bottleneck.

## Prerequisites

* .NET Framework 4.5

## Doxygen

Doxygen support is provided with Doxyfile in root of repository. Docs will be generated in "Docs" folder.
