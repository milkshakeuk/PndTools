using PndTools.Xml.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PndTools.Validation.Extensions
{
    public static class NonSchemaEnforcableValidationExtensions
    {
        public static void ValidateDefaultLocale(this IEnumerable<XElement> elements, List<string> errors)
        {
            PndTools.Guard.AgainstNullOrEmptyArgument(nameof(elements), elements);
            PndTools.Guard.AgainstNullArgument(nameof(errors), errors);
            elements.ToList().ForEach(element => Guard.AgainstIncorrectElement(element, "description", "title"));

            if(elements != null && elements.Any() && elements.All(e => e.Attribute<string>("lang") != "en_US"))
            {
                var parent = elements.First().Parent;
                var name = elements.First().Name.LocalName;

                errors.Add($@"PXML At least one '{name}' element with 'lang' attribute of value 'en_US' is required for the '{
                    parent.Name.LocalName}' element. ({parent.Position()})");
            }
        }

        public static void ValidateCategorySubcategoryPairing(this IEnumerable<XElement> elements, List<string> errors)
        {
            PndTools.Guard.AgainstNullOrEmptyArgument(nameof(elements), elements);
            PndTools.Guard.AgainstNullArgument(nameof(errors), errors);
            elements.ToList().ForEach(element => Guard.AgainstIncorrectElement(element, "category"));

            foreach (var category in elements)
            {
                var key = category.Attribute<string>("name");
                var subcategories = category.XElements("subcategory");

                var invalid = subcategories.Where(s => !CategoryMatrix[key].Contains(s.Attribute<string>("name"))
                                                    && !CategoryMatrix["spare"].Contains(s.Attribute<string>("name")));

                errors.AddRange(invalid.Select(s => $@"PXML The element 'subcategory' with name '{s.Attribute<string>("name")
                    }' is invalid for element 'category' with name '{key
                    }'. - See Free Desktop Standards for acceptable values. ({s.Position()})"));
            }
        }

        private static IDictionary<string, IEnumerable<string>> CategoryMatrix { get; } = 
            new Dictionary<string, IEnumerable<string>>()
        {
            { "AudioVideo", new [] {"Midi","Mixer","Music","Sequencer","Tuner","TV","AudioVideoEditing","Player","Recorder","DiscBurning"}},
            { "Audio", new [] {"HamRadio","Midi","Mixer","Sequencer","Tuner","AudioVideoEditing","Player","Recorder"}},
            { "Video", new [] {"TV","AudioVideoEditing","Player","Recorder"}},
            { "Development", new [] {"Building","Database","Debugger","IDE","GUIDesigner","Profiling","ProjectManagement","RevisionControl","Translation","WebDevelopment"}},
            { "Education", new [] {"Art","Construction","Music","Languages","Science","ArtificialIntelligence","Astronomy","Biology","Chemistry","ComputerScience","DataVisualization","Economy","Electricity","Geography","Geology","Geoscience","History","ImageProcessing","Literature","Math","NumericalAnalysis","MedicalSoftware","Physics","Robotics","Sports","ParallelComputing"}},
            { "Game", new [] {"ActionGame","AdventureGame","ArcadeGame","BoardGame","BlocksGame","CardGame","Emulator","KidsGame","LogicGame","RolePlaying","Simulation","SportsGame","StrategyGame"}},
            { "Graphics", new [] {"2DGraphics","VectorGraphics","RasterGraphics","3DGraphics","Scanning","OCR","Photography","Publishing","Viewer"}},
            { "Network", new [] {"Dialup","Email","InstantMessaging","Chat","IRCClient","FileTransfer","HamRadio","News","P2P","RemoteAccess","Telephony","VideoConference","WebBrowser","WebDevelopment"}},
            { "Office", new [] {"Calendar","ContactManagement","Database","Dictionary","Chart","Email","Finance","FlowChart","PDA","Photography","ProjectManagement","Presentation","Publishing","Spreadsheet","Viewer","WordProcessor"}},
            { "Settings", new [] {"DesktopSettings","HardwareSettings","Printing","PackageManager","Security"}},
            { "System", new [] {"Emulator","FileTools","FileManager","TerminalEmulator","Filesystem","Monitor","Security","Accessibility"}},
            { "Utility", new [] {"TextTools","TelephonyTools","Archiving","Compression","FileTools","FileManager","Accessibility","Calculator","Clock","TextEditor"}},
            { "QT", new [] {"KDE"}},
            { "GTK", new [] {"GNOME"}},
            { "spare", new [] {"Core","Documentation","GTK","Qt","Motif","Java","ConsoleOnly"} }
        };

        internal static class Guard
        {
            public static void AgainstIncorrectElement(XElement element, params string[] names)
            {
                if (!names.Contains(element.Name.LocalName))
                {
                    throw new InvalidElementException($"Expecting element of name in '{string.Join(", ", names)}', received element of name '{element.Name.LocalName}'");
                }
            }
        }
    }

    internal class InvalidElementException : Exception
    {
        public InvalidElementException(string message) : base(message)
        {
        }
    }
}
