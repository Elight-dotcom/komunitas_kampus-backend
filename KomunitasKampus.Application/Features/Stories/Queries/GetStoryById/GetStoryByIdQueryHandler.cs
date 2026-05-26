using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetStoryById;

public class GetStoryByIdQueryHandler
    : IRequestHandler<GetStoryByIdQuery, StoryDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPostRepository _postRepository;

    public GetStoryByIdQueryHandler(
        IStoryRepository storyRepository,
        IMembershipRepository membershipRepository,
        IPostRepository postRepository
    )
    {
        _storyRepository = storyRepository;
        _membershipRepository = membershipRepository;
        _postRepository = postRepository;
    }

    public async Task<StoryDto> Handle(
        GetStoryByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var story = await _storyRepository.GetByIdAsync(
            request.StoryId,
            cancellationToken
        );

        StoryAccessRules.EnsureStoryExists(story);
        StoryAccessRules.EnsureStoryIsActive(story!);

        var canView = await CanViewStoryAsync(
            story!,
            request.ViewerAccountId,
            cancellationToken
        );

        if (!canView)
        {
            throw new ForbiddenAccessAppException(
                "Kamu tidak memiliki akses untuk melihat story ini."
            );
        }

        return story!.ToStoryDto(request.ViewerAccountId);
    }

    private async Task<bool> CanViewStoryAsync(
        Domain.Entities.Story story,
        Guid? viewerAccountId,
        CancellationToken cancellationToken
    )
    {
        if (!viewerAccountId.HasValue)
        {
            return await OrganizationHasPublicFeedAsync(
                story.OrganizationId,
                cancellationToken
            );
        }

        var isOrganizationOwner =
            story.Organization?.AccountId == viewerAccountId.Value ||
            story.OrganizationId == viewerAccountId.Value;

        if (isOrganizationOwner)
        {
            return true;
        }

        var isAcceptedMember = await _membershipRepository.HasActiveMembershipAsync(
            viewerAccountId.Value,
            story.OrganizationId,
            cancellationToken
        );

        if (isAcceptedMember)
        {
            return true;
        }

        return await OrganizationHasPublicFeedAsync(
            story.OrganizationId,
            cancellationToken
        );
    }

    private async Task<bool> OrganizationHasPublicFeedAsync(
        Guid organizationId,
        CancellationToken cancellationToken
    )
    {
        var publicPosts = await _postRepository.GetFeedAsync(
            organizationId,
            viewerAccountId: null,
            page: 1,
            pageSize: 1,
            cancellationToken
        );

        return publicPosts.Count > 0;
    }
}
