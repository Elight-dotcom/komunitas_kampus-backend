namespace KomunitasKampus.Application.Features.Interactions.DTOs;

public sealed record LikeStatusDto(
    bool IsLiked,
    int LikeCount
);
