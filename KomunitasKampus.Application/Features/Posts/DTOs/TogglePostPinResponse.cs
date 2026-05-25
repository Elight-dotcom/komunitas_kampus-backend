namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record TogglePostPinResponse(
    Guid PostId,
    bool IsPinned,
    int? PinOrder
);
