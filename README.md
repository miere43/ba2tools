# Ba2Tools

Provides managed access to BA2 archives.

[Changelog](CHANGELOG.md).

Current features:
* Extract files from general archives, including packed files.
* Extract files from texture archives.
* Supports extracting to Stream, not only in file system.
* Support for extraction cancellation and progress reporting.
* BA2Archive instances are thread-safe.

Upcoming features:
* Write BA2 archives

Sample:
```c#
using (BA2Archive archive = BA2Loader.Load("Fallout4 - Interface.ba2")) {
    if (archive.TotalFiles >= 1) {
        archive.Extract(
                 fileName: archive.FileList[0],
              destination: "Modding/Interface/",
            overwriteFile: true);
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
| C#       | Ba2Tools v0.2.0   | 1 min 30 secs   |

Extraction speed test on Fallout4 - Sounds.ba2 (extract all files task)

| Language | Tool              | Extraction time | Notes
| -------- | ----------------- | --------------- | -----
| C++      | ba2extract v0003  | 0 min 20 secs   |
| C++      | B.A.E. v0.0.4     | 1 min 35 secs   | rough GUI measurement
| C#       | Ba2Tools v0.5.0   | 0 min 17 secs   | 

## Prerequisites

* .NET Framework 4.5
* C# 6.0

## Doxygen

Doxygen support is provided with Doxyfile in root of repository. Docs will be generated in "Docs" folder.