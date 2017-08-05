using System.Collections.Generic;

namespace PndTools.Models
{
    public class Pxml
    {
        public Package Package { get; set; }

        public IEnumerable<Application> Applications { get; set; }
    }

    public class Package
    {
        public string Id { get; set; }

        public Version Version { get; set; }

        public Author Author { get; set; }

        public IEnumerable<Title> Titles { get; set; }

        public IEnumerable<Description> Descriptions { get; set; }

        public Icon Icon { get; set; }
    }

    public class Application : Package
    {
        public IEnumerable<License> Licenses { get; set; }

        public IEnumerable<Pic> PreviewPics { get; set; }

        public Info Info { get; set; }

        public IEnumerable<Category> Categories { get; set; }
    }

    public class Author
    {
        public string Name { get; set; }

        public string Website { get; set; }
    }

    public class Version
    {
        public string Major { get; set; }

        public string Minor { get; set; }

        public string Release { get; set; }

        public string Build { get; set; }

        public string Type { get; set; }
    }

    public class Title
    {
        public string Lang { get; set; }

        public string Text { get; set; }
    }

    public class Description
    {
        public string Lang { get; set; }

        public string Text { get; set; }
    }


    public class Category
    {
        public string Name { get; set; }

        public Subcategory Subcategory { get; set; }
    }

    public class Subcategory
    {
        public string Name { get; set; }
    }

    public class License
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string SourceCodeUrl { get; set; }
    }

    public class Info
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }
    }

    public class Icon
    {
        public string Path { get; set; }
    }

    public class Pic
    {
        public string Path { get; set; }
    }
}
