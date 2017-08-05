using PndTools.Models;
using PndTools.Xml.Extensions;
using System.Xml.Linq;
using System.Collections.Generic;

namespace PndTools
{
    public class PxmlParser
    {
        public Pxml Parse(string xml)
        {
            var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var root = document.Root;

            return new Pxml
            {
                Package = GetPackage(root),
                Applications = root.XElements("application").List(GetApplication)
            };
        }

        private static Package GetPackage(XElement el)
        {
            var package = el.XElement("package");
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
                Licenses = application.XElement("licenses").XElements("license").List(GetLicense),
                Info = GetInfo(application.XElement("info")),
                PreviewPics = application.XElement("previewpics").XElements("pic").List(GetPic),
                Categories = application.XElement("categories").XElements("category").List(GetCategory)
            };
        }

        private static Icon GetIcon(XElement icon)
        {
            return new Icon
            {
                Path = icon.Attribute<string>("src")
            };
        }

        private static Pic GetPic(XElement pic)
        {
            return new Pic
            {
                Path = pic.Attribute<string>("src")
            };
        }

        private static IEnumerable<Title> GetTitles(XElement package)
        {
            if(package.XElement("titles")?.HasElements == true)
            {
                return package.XElement("titles")
                              .XElements("title")
                              .List(GetTitle);
            }

            // Pre HF6 compatibility
            return new[] {
                GetTitle(package.XElement("title"))
            };
        }

        private static Title GetTitle(XElement title)
        {
            return new Title
            {
                Lang = title.Attribute<string>("lang"),
                Text = title.Value<string>()
            };
        }

        private static Info GetInfo(XElement info)
        {
            return new Info
            {
                Name = info.Attribute<string>("name"),
                Type = info.Attribute<string>("type"),
                Path = info.Attribute<string>("src")
            };
        }

        private static IEnumerable<Description> GetDescriptions(XElement package)
        {
            if (package.XElement("descriptions")?.HasElements == true)
            {
                return package.XElement("descriptions")
                              .XElements("description")
                              .List(GetDescription);
            }

            // Pre HF6 compatibility
            return new[] {
                GetDescription(package.XElement("description"))
            };
        }

        private static Description GetDescription(XElement title)
        {
            return new Description
            {
                Lang = title.Attribute<string>("lang"),
                Text = title.Value<string>()
            };
        }

        private static Author GetAuthor(XElement author)
        {
            return new Author
            {
                Name = author.Attribute<string>("name"),
                Website = author.Attribute<string>("website")
            };
        }

        private static Version GetVersion(XElement version)
        {
            return new Version
            {
                Major = version.Attribute<string>("major"),
                Minor = version.Attribute<string>("minor"),
                Release = version.Attribute<string>("release"),
                Build = version.Attribute<string>("build"),
                Type = version.Attribute<string>("type")
            };
        }

        private static License GetLicense(XElement version)
        {
            return new License
            {
                Name = version.Attribute<string>("name"),
                Url = version.Attribute<string>("url"),
                SourceCodeUrl = version.Attribute<string>("sourcecodeurl")
            };
        }

        private static Category GetCategory(XElement category)
        {
            return new Category
            {
                Name = category.Attribute<string>("name"),
                Subcategory = GetSubcategory(category.XElement("subcategory"))
            };
        }

        private static Subcategory GetSubcategory(XElement subcategory)
        {
            return new Subcategory
            {
                Name = subcategory.Attribute<string>("name")
            };
        }
    }
}
