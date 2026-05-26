namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record DeletePostResponse(
    Guid PostId,
    string Message
);
