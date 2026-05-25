namespace KomunitasKampus.API.Contracts.Posts;

public sealed record UpdatePostRequest(
    string Title,
    string? Caption
);
