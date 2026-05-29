using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

/// <summary>
/// Interface kecil khusus Chat untuk lookup Account.
/// Dibutuhkan Application layer agar bisa validasi role organization
/// pada CreateSubGroupCommand tanpa akses DbContext langsung.
/// </summary>
public interface IChatAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);
}
