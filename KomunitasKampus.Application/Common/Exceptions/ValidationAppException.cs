namespace KomunitasKampus.Application.Common.Exceptions;

public class ValidationAppException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IReadOnlyDictionary<string, string[]> errors)
        : base("Terjadi kesalahan validasi.")
    {
        Errors = errors;
    }
}