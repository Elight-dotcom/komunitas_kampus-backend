using System.Text.Json.Serialization;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.API.Contracts.Interactions;

public sealed record SharePostRequest(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    SharePlatform Platform
);
