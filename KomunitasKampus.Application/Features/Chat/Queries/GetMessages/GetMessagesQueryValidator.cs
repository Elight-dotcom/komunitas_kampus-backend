using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Queries.GetMessages;

public class GetMessagesQueryValidator : AbstractValidator<GetMessagesQuery>
{
    public GetMessagesQueryValidator()
    {
        RuleFor(query => query.RoomId).NotEmpty().WithMessage("RoomId tidak boleh kosong.");
        RuleFor(query => query.RequestingAccountId).NotEmpty().WithMessage("RequestingAccountId tidak boleh kosong.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1).WithMessage("Page minimal 1.");
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize harus di antara 1 sampai 100.");
    }
}
