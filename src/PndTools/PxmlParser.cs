using PndTools.Models;
using PndTools.Xml.Extensions;
using System.Xml.Linq;

namespace PndTools;

/// <summary>Parses a PXML string into a <see cref="Pxml"/> object graph.</summary>
public class PxmlParser
{
    /// <summary>Parses a PXML string into a <see cref="Pxml"/> object graph.</summary>
    /// <param name="xml">The PXML string to parse.</param>
    /// <returns>The parsed <see cref="Pxml"/>.</returns>
    /// <exception cref="System.Xml.XmlException"><paramref name="xml"/> is not valid XML.</exception>
    public Pxml Parse(string xml)
    {
        var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var root = document.Root!;

        return new Pxml
        {
            Package = GetPackage(root),
            Applications = root.XElements("application").List(GetApplication)
        };
    }

    private static Package GetPackage(XElement el)
    {
        var package = el.XElement("package")!;
        return new Package
        {
            Id = package.Attribute<string>("id"),
            Version = GetVersion(package.XElement("version")),
            Author = GetAuthor(package.XElement("author")),
            Titles = GetTitles(package),
            Descriptions = GetDescriptions(package),
            Icon = GetIcon(package.XElement("icon"))
        };
    }

    private static Application GetApplication(XElement application)
    {
        return new Application
        {
            Id = application.Attribute<string>("id"),
            Version = GetVersion(application.XElement("version")),
            Author = GetAuthor(application.XElement("author")),
            Titles = GetTitles(application),
            Descriptions = GetDescriptions(application),
            Icon = GetIcon(application.XElement("icon")),
            Licenses = application.XElement("licenses")!.XElements("license").List(GetLicense),
            Info = GetInfo(application.XElement("info")),
            PreviewPics = application.XElement("previewpics")!.XElements("pic").List(GetPic),
            Categories = application.XElement("categories")!.XElements("category").List(GetCategory)
        };
    }

    private static Icon? GetIcon(XElement? icon)
    {
        return icon is null ? null : new Icon(icon.Attribute<string>("src"));
    }

    private static Pic GetPic(XElement pic)
    {
        return new(pic.Attribute<string>("src"));
    }

    private static IReadOnlyList<Title> GetTitles(XElement package)
    {
        if (package.XElement("titles")?.HasElements == true)
        {
            return package.XElement("titles")!.XElements("title").List(GetTitle);
        }

        // Pre-HF6 compatibility
        return [GetTitle(package.XElement("title")!)];
    }

    private static Title GetTitle(XElement title)
    {
        return new(title.Attribute<string>("lang"), title.Value<string>());
    }

    private static Info? GetInfo(XElement? info)
    {
        return info is null ? null : new Info(info.Attribute<string>("name"), info.Attribute<string>("type"), info.Attribute<string>("src"));
    }

    private static IReadOnlyList<Description> GetDescriptions(XElement package)
    {
        if (package.XElement("descriptions")?.HasElements == true)
        {
            return package.XElement("descriptions")!.XElements("description").List(GetDescription);
        }

        // Pre-HF6 compatibility
        return [GetDescription(package.XElement("description")!)];
    }

    private static Description GetDescription(XElement desc)
    {
        return new(desc.Attribute<string>("lang"), desc.Value<string>());
    }

    private static Author? GetAuthor(XElement? author)
    {
        return author is null ? null : new Author(author.Attribute<string>("name"), author.Attribute<string>("website"));
    }

    private static PxmlVersion? GetVersion(XElement? version)
    {
        return version is null ? null : new PxmlVersion(
            version.Attribute<string>("major"),
            version.Attribute<string>("minor"),
            version.Attribute<string>("release"),
            version.Attribute<string>("build"),
            version.Attribute<string>("type"));
    }

    private static License GetLicense(XElement license)
    {
        return new(license.Attribute<string>("name"), license.Attribute<string>("url"), license.Attribute<string>("sourcecodeurl"));
    }

    private static Category GetCategory(XElement category)
    {
        return new(category.Attribute<string>("name"), GetSubcategory(category.XElement("subcategory")));
    }

    private static Subcategory? GetSubcategory(XElement? subcategory)
    {
        return subcategory is null ? null : new Subcategory(subcategory.Attribute<string>("name"));
    }
}
