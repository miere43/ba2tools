# Ba2Tools

Provides managed access to BA2 archives.

Current features:
* Extract files from general archives, including packed files.
* Extract files from texture archives.
* List files in archive

Upcoming features:
* Write BA2 archives

Sample:
```c#
var archive = Ba2ArchiveLoader.Load("D:\\Games\\Steam\\steamapps\\Fallout 4\\Data\\Fallout 4 - Interface.ba2");
string[] filesInArchive = archive.ListFiles();

if (filesInArchive.Length > 1)
{
	archive.Extract(filesInArchive[0], "D:\\Modding\\Fallout 4 Data\\Interface");
}
```

[Show me more samples](Samples/).

## Prerequisites

* .NET Framework 4

## Doxygen

Doxygen support is provided with Doxyfile in root of repository. Docs will be generated in "Docs" folder.