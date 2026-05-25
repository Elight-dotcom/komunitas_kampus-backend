namespace KomunitasKampus.Application.Common.Exceptions;

public class NotFoundAppException : Exception
{
    public NotFoundAppException(string message = "Data tidak ditemukan.")
        : base(message)
    {
    }
}
