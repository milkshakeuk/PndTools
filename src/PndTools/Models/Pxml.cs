// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools.Models;

/// <summary>Represents the parsed contents of a PXML document.</summary>
public record Pxml
{
    /// <summary>The package-level metadata shared across all applications in this PND.</summary>
    public required Package Package { get; init; }

    /// <summary>The individual applications bundled in this PND.</summary>
    public required IReadOnlyList<Application> Applications { get; init; }
}

/// <summary>Package-level metadata present in a PXML document.</summary>
public record Package
{
    /// <summary>The unique identifier for this package.</summary>
    public string? Id { get; init; }

    /// <summary>The version of this package.</summary>
    public PxmlVersion? Version { get; init; }

    /// <summary>The author of this package.</summary>
    public Author? Author { get; init; }

    /// <summary>The localised titles for this package. At least one entry with <c>lang="en_US"</c> is required.</summary>
    public IReadOnlyList<Title> Titles { get; init; } = [];

    /// <summary>The localised descriptions for this package. At least one entry with <c>lang="en_US"</c> is required.</summary>
    public IReadOnlyList<Description> Descriptions { get; init; } = [];

    /// <summary>The icon associated with this package.</summary>
    public Icon? Icon { get; init; }
}

/// <summary>An individual application bundled within a PND, extending the package-level metadata.</summary>
public record Application : Package
{
    /// <summary>The licences that apply to this application.</summary>
    public IReadOnlyList<License> Licenses { get; init; } = [];

    /// <summary>Preview images for this application.</summary>
    public IReadOnlyList<Pic> PreviewPics { get; init; } = [];

    /// <summary>The executable information for this application.</summary>
    public Info? Info { get; init; }

    /// <summary>The FreeDesktop.org categories assigned to this application.</summary>
    public IReadOnlyList<Category> Categories { get; init; } = [];
}

/// <summary>The author of a package or application.</summary>
/// <param name="Name">The author's display name.</param>
/// <param name="Website">The author's website URL.</param>
public record Author(string? Name, string? Website);

/// <summary>The version of a package or application.</summary>
/// <param name="Major">The major version component.</param>
/// <param name="Minor">The minor version component.</param>
/// <param name="Release">The release version component.</param>
/// <param name="Build">The build version component.</param>
/// <param name="Type">The release type, e.g. <c>release</c> or <c>beta</c>.</param>
public record PxmlVersion(string? Major, string? Minor, string? Release, string? Build, string? Type);

/// <summary>A localised title string.</summary>
/// <param name="Lang">The locale identifier, e.g. <c>en_US</c>.</param>
/// <param name="Text">The title text.</param>
public record Title(string? Lang, string? Text);

/// <summary>A localised description string.</summary>
/// <param name="Lang">The locale identifier, e.g. <c>en_US</c>.</param>
/// <param name="Text">The description text.</param>
public record Description(string? Lang, string? Text);

/// <summary>A FreeDesktop.org category assigned to an application.</summary>
/// <param name="Name">The category name, e.g. <c>Game</c>.</param>
/// <param name="Subcategory">The optional subcategory.</param>
public record Category(string? Name, Subcategory? Subcategory);

/// <summary>A FreeDesktop.org subcategory.</summary>
/// <param name="Name">The subcategory name, e.g. <c>ActionGame</c>.</param>
public record Subcategory(string? Name);

/// <summary>A licence that applies to an application.</summary>
/// <param name="Name">The licence name, e.g. <c>GPL</c>.</param>
/// <param name="Url">A URL to the licence text.</param>
/// <param name="SourceCodeUrl">A URL to the application's source code.</param>
public record License(string? Name, string? Url, string? SourceCodeUrl);

/// <summary>Executable information for an application.</summary>
/// <param name="Name">The executable name.</param>
/// <param name="Type">The executable type, e.g. <c>gui</c>.</param>
/// <param name="Path">The path to the executable within the PND archive.</param>
public record Info(string? Name, string? Type, string? Path);

/// <summary>An icon associated with a package or application.</summary>
/// <param name="Path">The path to the icon within the PND archive.</param>
public record Icon(string? Path);

/// <summary>A preview image associated with an application.</summary>
/// <param name="Path">The path to the image within the PND archive.</param>
public record Pic(string? Path);
