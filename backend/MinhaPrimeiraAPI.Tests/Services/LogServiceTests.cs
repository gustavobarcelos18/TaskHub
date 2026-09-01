using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Tests.Services;

public sealed class LogServiceTests
{
    [Fact]
    public async Task ConsultarAsync_DeveExporSomenteContratoSeguroECalcularPaginas()
    {
        var repository = new FakeLogRepository
        {
            Itens = [new LogEvento { Id = 1, TimestampUtc = DateTime.UtcNow, Level = "Error", EventName = "Falha", Message = "mensagem", SafePropertiesJson = "{\"TarefaId\":\"42\"}" }],
            Total = 3
        };

        var resultado = await new LogService(repository).ConsultarAsync(new ConsultaLogsRequest { Page = 2, PageSize = 2 });

        Assert.Equal(2, resultado.TotalPaginas);
        Assert.Equal("42", Assert.Single(resultado.Itens).Properties["TarefaId"]);
        Assert.DoesNotContain("Password", resultado.Itens[0].Properties.Keys);
    }

    private sealed class FakeLogRepository : ILogRepository
    {
        public List<LogEvento> Itens { get; init; } = [];
        public int Total { get; init; }
        public Task<(List<LogEvento> Itens, int Total)> ConsultarAsync(ConsultaLogsRequest consulta, CancellationToken cancellationToken = default) => Task.FromResult((Itens, Total));
    }
}
