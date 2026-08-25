using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Data;

namespace MinhaPrimeiraAPI.Tests.Infrastructure;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private DbContextOptions<AppDbContext>? _options;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        await using var command = _connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public AppDbContext CreateContext()
    {
        return new AppDbContext(_options ?? throw new InvalidOperationException("O banco de teste não foi inicializado."));
    }

    public async Task<bool> ForeignKeysEnabledAsync()
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys;";
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
