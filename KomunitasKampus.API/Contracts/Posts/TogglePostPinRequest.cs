namespace KomunitasKampus.API.Contracts.Posts;

public sealed record TogglePostPinRequest(
    bool IsPinned,
    int? PinOrder
);
