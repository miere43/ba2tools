## Version 0.6
* BA2Archive instances are thread-safe now.
* Changed signature of some of extraction methods, replaced ```BA2Archive.ListFiles() -> IList<string>``` methods to ```BA2Archive.FileList -> IReadOnlyList<string>``` property.
* BA2Archive now throws ObjectDisposedException when archive is disposed and extraction method of that archive was used.

## Version 0.5
* Added multi-threaded extraction. Use ```BA2Loader.Load(path, BA2LoaderFlags.Multithreaded)``` to enable multithreaded extraction for ```ExtractFiles``` and ```ExtractAll``` methods.
* Added new overloads to ```ExtractToStream```, ```Extract```, ```ExtractFiles``` methods accepting as first parameter file index/file indexes in archive instead of filename/filenames in archive.

## Version 0.4.1
* Fixed bug when using ExtractFiles(...) methods in general archive that was leading to null exceptions.

## Version 0.4
* Performance improvements for extracting files from archives.
* Added generic variants of BA2Loader.Load(...) methods.
* Changed BA2Loader.Load(Stream stream...) method access level to public.
* Now BA2Loader.Load(...) disposes archive when any exception occurs.

## Version 0.3
* More generic method declarations (string[] => IEnumerable<string>)
* File entries are classes now instead of structs.
* Added possibility of cancelling extraction and reporting progress of extraction.

## Version 0.2
* Upgraded to .NET Framework 4.5
* Added Doxygen support.
* Added tests.
* Added texture archive support.
* Updated samples.
* Fixed invalid unpacking of packed general archive files.
* Fixed handling archives with invalid name table offset.
* Renamed Ba2* to BA2*
* BA2Archives are distinct from file system now: they can be constructed from plain streams now.
* BA2Archives now holding a stream to archive data and implements ```IDisposable``` interface. Now you should call ```Dispose()``` when BA2Archive instance is not needed anymore.

## Version 0.1
* Partial support of general archives: exported packed files are invalid.