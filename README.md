# Ba2Tools

Provides managed access to BA2 archives.

Current features:
* Extract files from general archives
* List files in archive

Upcoming features:
* Extract files from texture archives

Sample:
```c#
var archive = Ba2ArchiveLoader.Load("D:\Games\Steam\steamapps\Fallout 4\Data\Fallout 4 - Interface.ba2");
string[] filesInArchive = archive.ListFiles();

if (filesInArchive.Length > 1)
{
	archive.Extract(filesInArchive[0], "D:\\Modding\\Fallout 4 Data\\Interface");
}
```