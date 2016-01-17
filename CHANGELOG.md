## Version 0.2.0.0
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

## Version 0.1.0.0
* Initial release.
* Partial support of general archives: exported packed files are invalid.