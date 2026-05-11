// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace PndTools.Models;

/// <summary>
/// AOT-safe JSON serialisation context for all PXML model types.
/// Use with <see cref="System.Text.Json.JsonSerializer"/> source generation to avoid reflection at runtime.
/// </summary>
[JsonSerializable(typeof(Pxml))]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(Application))]
[JsonSerializable(typeof(Author))]
[JsonSerializable(typeof(PxmlVersion))]
[JsonSerializable(typeof(Title))]
[JsonSerializable(typeof(Description))]
[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(Subcategory))]
[JsonSerializable(typeof(License))]
[JsonSerializable(typeof(Info))]
[JsonSerializable(typeof(Icon))]
[JsonSerializable(typeof(Pic))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class PndToolsJsonContext : JsonSerializerContext;
