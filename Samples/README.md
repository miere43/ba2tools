# Samples

These folders contain samples for BA2Tools library.

You can also refer to these small samples as well:

## Extract all files
```c#
using (var archive = BA2Loader.Load("Fallout4 - Startup.ba2")) {
	archive.ExtractAll("Extract/StartupArchive/", overwriteFiles: true);
}
```

## Get total files
```c#
using (var archive = BA2Loader.Load("Fallout4 - Startup.ba2")) {
	Console.WriteLine($"Total files in archive: {archive.TotalFiles}");
}
```

## Get archive type
```c#
var archive = BA2Loader.Load("Fallout4 - Startup.ba2"));

if (archive.GetType() == typeof(BA2GeneralArchive)) {
	Console.WriteLine("This archive can contain any files.");
} else if (archive as BA2TextureArchive != null) {
	Console.WriteLine("This archive contains only textures.");
} else {
	var archiveType = BA2Loader.GetArchiveType(archive);
	Console.WriteLine($"Can't say anything about {archiveType} archive type.");
}

archive.Dispose();
```

## Extract to stream
```c#
using (var stream = new System.IO.MemoryStream()) {
	using (var textureArchive = BA2Loader.Load("Fallout4 - Textures1.ba2")) {
		string firstFilePath = textureArchive.FileList[0];

		textureArchive.ExtractToStream(firstFilePath, stream, true);
	}

	// ensure proper position.
	stream.Seek(0, SeekOrigin.Begin);

	// perform some actions on texture.
	// ...
}	
```

## Async example
```
public Task ExtractAllAsync(BA2Archive archive)
{
    return Task.Run(() => archive.ExtractAll("D:/stuff", true));
}
```