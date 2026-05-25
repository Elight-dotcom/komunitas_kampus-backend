namespace KomunitasKampus.Domain.Enums;

public enum AccountRole
{
    Organisasi = 1,
    Mahasiswa = 2
}

public enum PostVisibility
{
    Internal = 1,
    Private = 2,
    Public = 3
}

public enum PostMediaType
{
    Image = 1,
    Video = 2,
    Document = 3
}

public enum PostMediaStatus
{
    Loading = 1,
    Ready = 2
}

public enum MembershipStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}

public enum MembershipInviteType
{
    Invite = 1,
    Request = 2
}

public enum StoryMediaType
{
    Image = 1,
    Video = 2,
    Text = 3
}

public enum ChatRoomType
{
    Main = 1,
    Sub = 2,
    Direct = 3
}

public enum SharePlatform
{
    Internal = 1,
    External = 2
}