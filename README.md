# PndTools [![Build Status][travis-image]][travis-url]



PndTools is a DotNet library for handling pnd files

## Goals
* Easy to use library
* Cross platform
* Use in repoV2???

## Features
* Find and extract the pXML data
* Find and extract the icon data

### Basic Usage

```csharp
string result;
using (Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd"))
{
    result = PndStreamHelper.GetPxml(stream);
}
```
```csharp
byte[] result;
using (Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd"))
{
    result = PndStreamHelper.GetIcon(stream);
}
```

## ToDo
* Get file details (name, path, directory, extension, file size etc.)
* Basic Pnd file validation based on file extension and file type
* Detect Pnd archive type `( ISO | Squashfs )`
* Save the `PXML` to its own file
* Save the icon to its own file
* Ability to list files in archive
* Ability to extract files from both pnd archive formats `( ISO | Squashfs )`
* Validate the `PXML`

### Unit Testing

The library is accompanied by unit tests. The library uses `xUnit` for testing.

[Learn about xUnit](https://xunit.github.io/)

## Community

This is for the OpenPandora community mainly please check them out.
[Visit the OpenPandora Community](http://boards.openpandora.org/)

## License

The PndAid library is released under the LGPL v2.1 license.

<http://www.gnu.org/licenses/lgpl-2.1.html>

[travis-url]: https://travis-ci.org/milkshakeuk/PndTools
[travis-image]: https://travis-ci.org/milkshakeuk/PndTools.svg?branch=master