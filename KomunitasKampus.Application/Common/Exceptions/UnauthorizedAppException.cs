namespace KomunitasKampus.Application.Common.Exceptions;

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message = "Email/username atau password salah.")
        : base(message)
    {
    }
}