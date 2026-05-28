// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Xml.Linq;
using PndTools.Xml.Extensions;

namespace PndTools.Validation.Extensions;

/// <summary>
/// Validation rules for PXML that cannot be expressed in XSD, such as locale requirements
/// and FreeDesktop.org category/subcategory pairings.
/// </summary>
public static class NonSchemaEnforceableValidationExtensions
{
    /// <summary>
    /// Validates that at least one element in <paramref name="elements"/> has a <c>lang</c>
    /// attribute of <c>en_US</c>, appending an error to <paramref name="errors"/> if not.
    /// </summary>
    /// <param name="elements">The title or description elements to validate.</param>
    /// <param name="errors">The list to append any validation errors to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <c>null</c>.</exception>
    public static void ValidateDefaultLocale(this IEnumerable<XElement> elements, List<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentCollectionException.ThrowIfNullOrEmpty(elements);
        foreach (var element in elements)
        {
            InvalidElementException.ThrowIfIncorrectElement(element, "description", "title");
        }

        if (elements.All(e => e.Attribute<string>("lang") != "en_US"))
        {
            var first = elements.First();
            var parent = first.Parent!;
            errors.Add($"PXML At least one '{first.Name.LocalName}' element with 'lang' attribute of value 'en_US' is required for the '{parent.Name.LocalName}' element. ({parent.Position()})");
        }
    }

    /// <summary>
    /// Validates that each subcategory in <paramref name="elements"/> is a valid pairing for its
    /// parent category according to the FreeDesktop.org specification, appending errors to
    /// <paramref name="errors"/> for any invalid pairings.
    /// </summary>
    /// <param name="elements">The category elements to validate.</param>
    /// <param name="errors">The list to append any validation errors to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <c>null</c>.</exception>
    public static void ValidateCategorySubcategoryPairing(this IEnumerable<XElement> elements, List<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentCollectionException.ThrowIfNullOrEmpty(elements);
        foreach (var element in elements)
        {
            InvalidElementException.ThrowIfIncorrectElement(element, "category");
        }

        foreach (var category in elements)
        {
            var key = category.Attribute<string>("name");
            if (key is null || !_categoryMatrix.ContainsKey(key))
            {
                continue;
            }

            var subcategories = category.XElements("subcategory");
            var spare = _categoryMatrix["spare"];

            var invalid = subcategories.Where(s =>
            {
                var name = s.Attribute<string>("name");
                return name is not null
                    && !_categoryMatrix[key].Contains(name)
                    && !spare.Contains(name);
            });

            errors.AddRange(invalid.Select(s =>
                $"PXML The element 'subcategory' with name '{s.Attribute<string>("name")}' is invalid for element 'category' with name '{key}'. - See Free Desktop Standards for acceptable values. ({s.Position()})"));
        }
    }

#pragma warning disable CA1859 // IReadOnlyDictionary signals immutability intent on this field
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _categoryMatrix =
        new Dictionary<string, IReadOnlySet<string>>
#pragma warning restore CA1859
        {
            { "AudioVideo", new HashSet<string> { "Midi","Mixer","Music","Sequencer","Tuner","TV","AudioVideoEditing","Player","Recorder","DiscBurning" } },
            { "Audio",      new HashSet<string> { "HamRadio","Midi","Mixer","Sequencer","Tuner","AudioVideoEditing","Player","Recorder" } },
            { "Video",      new HashSet<string> { "TV","AudioVideoEditing","Player","Recorder" } },
            { "Development",new HashSet<string> { "Building","Database","Debugger","IDE","GUIDesigner","Profiling","ProjectManagement","RevisionControl","Translation","WebDevelopment" } },
            { "Education",  new HashSet<string> { "Art","Construction","Music","Languages","Science","ArtificialIntelligence","Astronomy","Biology","Chemistry","ComputerScience","DataVisualization","Economy","Electricity","Geography","Geology","Geoscience","History","ImageProcessing","Literature","Math","NumericalAnalysis","MedicalSoftware","Physics","Robotics","Sports","ParallelComputing" } },
            { "Game",       new HashSet<string> { "ActionGame","AdventureGame","ArcadeGame","BoardGame","BlocksGame","CardGame","Emulator","KidsGame","LogicGame","RolePlaying","Simulation","SportsGame","StrategyGame" } },
            { "Graphics",   new HashSet<string> { "2DGraphics","VectorGraphics","RasterGraphics","3DGraphics","Scanning","OCR","Photography","Publishing","Viewer" } },
            { "Network",    new HashSet<string> { "Dialup","Email","InstantMessaging","Chat","IRCClient","FileTransfer","HamRadio","News","P2P","RemoteAccess","Telephony","VideoConference","WebBrowser","WebDevelopment" } },
            { "Office",     new HashSet<string> { "Calendar","ContactManagement","Database","Dictionary","Chart","Email","Finance","FlowChart","PDA","Photography","ProjectManagement","Presentation","Publishing","Spreadsheet","Viewer","WordProcessor" } },
            { "Settings",   new HashSet<string> { "DesktopSettings","HardwareSettings","Printing","PackageManager","Security" } },
            { "System",     new HashSet<string> { "Emulator","FileTools","FileManager","TerminalEmulator","Filesystem","Monitor","Security","Accessibility" } },
            { "Utility",    new HashSet<string> { "TextTools","TelephonyTools","Archiving","Compression","FileTools","FileManager","Accessibility","Calculator","Clock","TextEditor" } },
            { "QT",         new HashSet<string> { "KDE" } },
            { "GTK",        new HashSet<string> { "GNOME" } },
            { "spare",      new HashSet<string> { "Core","Documentation","GTK","Qt","Motif","Java","ConsoleOnly" } }
        };

}

internal class InvalidElementException : Exception
{
    public InvalidElementException() { }
    public InvalidElementException(string message) : base(message) { }
    public InvalidElementException(string message, Exception inner) : base(message, inner) { }

    public static void ThrowIfIncorrectElement(XElement element, params ReadOnlySpan<string> names)
    {
        if (!names.Contains(element.Name.LocalName))
        {
            throw new InvalidElementException($"Expecting element of name in '{string.Join(", ", names.ToArray())}', received element of name '{element.Name.LocalName}'");
        }
    }
}
