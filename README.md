# Moriyama.PreviewPDF

Moriyama.PreviewPDF are packages which automatically generates a thumbnail png when uploading a PDF to the Umbraco CMS Media section.

## Compatibility

### This package will **only work on Windows machines and servers**

This has been tested and compiled against **Umbraco V10 (DotNet 6)** and **Umbraco V13 (DotNet 8)** no other versions are officially supported.

There are two versions of the package

## Ghostscript
It makes use of the [Ghostscript](https://www.ghostscript.com/) library, when installing the NuGet package it will add both the Windows x86 and x64 DLLs into your project.

The current version used is: 10.02.1

This package also uses the prerelease version of [Ghostscript.Core](https://github.com/porrey/Ghostscript.NET)

Please review the licensing which can be found here:
[https://www.ghostscript.com/licensing/index.html](https://www.ghostscript.com/licensing/index.html)

## ABCpdf
It makes use of the [ABCpdf](https://www.websupergoo.com/abcpdf-1.aspx) library

You will need ti purchase a license, you can puchase that here:
[https://www.ghostscript.com/licensing/index.html](https://www.ghostscript.com/licensing/index.html)

## Getting started

You will need to edit the **Article** Media Type, this can be found under **Settings -> Media Types -> Article**

You will need to add a property called `Thumbnail` with the alias of `thumbnail` and Editor type of **Upload**

Once added change the order so that **Thumbnail** is the first property

![media type configuration](/Screenshots/media-type-configuration.png "media type configuration")

### Contribution guidelines

To raise a new bug, create an issue on the GitHub repository. To fix a bug or add new features, fork the repository and send a pull request with your changes. Feel free to add ideas to the repository's issues list if you would to discuss anything related to the package.

### Who do I talk to?
This project is maintained by [Moriyama](https://moriyama) and contributors. If you have any questions about the project please raise an issue on GitHub.

## License

Copyright &copy; 2024 [Moriyama](https://moriyama.co.uk), and other contributors

Licensed under the MIT License