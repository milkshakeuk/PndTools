// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection.Extensions;
using PndTools.AspNetCore;
using PndTools.Validation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extension methods for registering PndTools services with an <see cref="IServiceCollection"/>.</summary>
public static class PndToolsServiceCollectionExtensions
{
    /// <summary>
    /// Registers PndTools services as Singletons: <see cref="PndTools.IPxmlParser"/>,
    /// <see cref="IPxmlValidator"/>, and <see cref="IPndArchiveFactory"/>.
    /// </summary>
    /// <remarks>
    /// This method is idempotent — calling it more than once has no effect on existing
    /// registrations, and will not override any registrations already present in
    /// <paramref name="services"/>.
    /// </remarks>
    /// <param name="services">The service collection to add PndTools services to.</param>
    /// <returns><paramref name="services"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddPndTools(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<PndTools.IPxmlParser, PndTools.PxmlParser>();
        services.TryAddSingleton<IPxmlValidator, PxmlValidator>();
        services.TryAddSingleton<IPndArchiveFactory, PndArchiveFactory>();

        return services;
    }
}
