using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Queries.GetRoomList;

public class GetRoomListQueryValidator : AbstractValidator<GetRoomListQuery>
{
    public GetRoomListQueryValidator()
    {
        RuleFor(query => query.AccountId).NotEmpty().WithMessage("AccountId tidak boleh kosong.");
    }
}
