namespace KomunitasKampus.Application.Common.Exceptions;

public class ForbiddenAccessAppException : Exception
{
    public ForbiddenAccessAppException(string message = "Kamu tidak memiliki akses untuk melakukan aksi ini.")
        : base(message)
    {
    }
}
