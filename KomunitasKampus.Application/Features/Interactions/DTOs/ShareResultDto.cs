namespace KomunitasKampus.Application.Features.Interactions.DTOs;

public sealed record ShareResultDto(
    int ShareCount,
    bool RequiresAuth
);
