namespace KomunitasKampus.Infrastructure.Realtime;

public static class RealtimeGroups
{
    public static string Account(Guid accountId)
    {
        return $"account:{accountId}";
    }
}
