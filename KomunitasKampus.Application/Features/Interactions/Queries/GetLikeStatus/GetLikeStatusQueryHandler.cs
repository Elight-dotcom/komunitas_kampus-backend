using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetLikeStatus;

public class GetLikeStatusQueryHandler
    : IRequestHandler<GetLikeStatusQuery, LikeStatusDto>
{
    private readonly ILikeRepository _likeRepository;

    public GetLikeStatusQueryHandler(ILikeRepository likeRepository)
    {
        _likeRepository = likeRepository;
    }

    public async Task<LikeStatusDto> Handle(
        GetLikeStatusQuery request,
        CancellationToken cancellationToken
    )
    {
        var isLiked = await _likeRepository.IsLikedByUserAsync(
            request.UserId,
            request.PostId,
            cancellationToken
        );

        var likeCount = await _likeRepository.GetLikeCountAsync(
            request.PostId,
            cancellationToken
        );

        return new LikeStatusDto(isLiked, likeCount);
    }
}
