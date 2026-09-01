using Microsoft.Data.Sqlite;
using ProjetoTarefas.Extensions;
using Serilog.Events;

namespace ProjetoTarefas.Tests.Infrastructure;

public sealed class SqliteLogSinkTests
{
    [Fact]
    public void Emit_DevePersistirUmEventoSeguroSemGerarEventoDeRetorno()
    {
        var caminho = Path.Combine(Path.GetTempPath(), $"taskhub-sink-{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = caminho, Pooling = false }.ToString();
            using var sink = new SqliteLogSink(connectionString, 30);
            sink.Emit(new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Error, null, new MessageTemplate("Falha", []),
            [new LogEventProperty("EventName", new ScalarValue("TesteSink")), new LogEventProperty("UserId", new ScalarValue("user-1")), new LogEventProperty("TraceId", new ScalarValue("trace-1")), new LogEventProperty("TarefaId", new ScalarValue(42)), new LogEventProperty("Password", new ScalarValue("não persistir"))]));

            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*), SafePropertiesJson FROM LogEventos";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal(1, reader.GetInt32(0));
            Assert.Contains("TarefaId", reader.GetString(1));
            Assert.DoesNotContain("Password", reader.GetString(1));
        }
        finally { SqliteConnection.ClearAllPools(); if (File.Exists(caminho)) File.Delete(caminho); }
    }
}
