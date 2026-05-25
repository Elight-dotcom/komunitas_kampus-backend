namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record UpdatePostDto(
    string Title,
    string? Caption
);
