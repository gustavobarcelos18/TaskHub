using Microsoft.Data.Sqlite;

if (!ArgumentosSaoValidos(args))
{
    Console.Error.WriteLine(
        "Uso: backup <banco> <diretorio> | restore <backup> <banco> | integrity-check <banco>"
    );
    return 1;
}

if (args[0] == "integrity-check")
{
    return VerificarIntegridade(Path.GetFullPath(args[1]));
}

var operacao = args[0];
var origem = Path.GetFullPath(args[1]);

if (!File.Exists(origem))
{
    Console.Error.WriteLine($"Arquivo não encontrado: {origem}");
    return 1;
}

if (VerificarIntegridade(origem) != 0)
{
    Console.Error.WriteLine("A operação foi cancelada porque o banco de origem não está íntegro.");
    return 1;
}

var destino = operacao == "backup"
    ? Path.Combine(
        Path.GetFullPath(args[2]),
        $"tarefas-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.db"
    )
    : Path.GetFullPath(args[2]);

if (string.Equals(origem, destino, StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("O banco de origem e o destino precisam ser arquivos diferentes.");
    return 1;
}

var diretorioDestino = Path.GetDirectoryName(destino);

if (string.IsNullOrWhiteSpace(diretorioDestino))
{
    Console.Error.WriteLine($"Não foi possível determinar o diretório de destino: {destino}");
    return 1;
}

Directory.CreateDirectory(diretorioDestino);

if (operacao == "restore" && File.Exists(destino))
{
    var bancoPreservado = Path.Combine(
        diretorioDestino,
        $"tarefas-pre-restore-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.db"
    );
    CopiarBanco(destino, bancoPreservado);
    Console.WriteLine($"Banco anterior preservado em: {bancoPreservado}");
}

CopiarBanco(origem, destino);
Console.WriteLine(operacao == "backup" ? $"Backup criado: {destino}" : $"Backup restaurado em: {destino}");
return 0;

static bool ArgumentosSaoValidos(string[] argumentos)
{
    var operacaoComDoisCaminhos =
        argumentos.Length == 3 && argumentos[0] is "backup" or "restore";
    var verificacaoIntegridade =
        argumentos.Length == 2 && argumentos[0] == "integrity-check";

    return operacaoComDoisCaminhos || verificacaoIntegridade;
}

static int VerificarIntegridade(string caminhoBanco)
{
    if (!File.Exists(caminhoBanco))
    {
        Console.Error.WriteLine($"Arquivo não encontrado: {caminhoBanco}");
        return 1;
    }

    using var conexao = new SqliteConnection(CriarConnectionString(caminhoBanco, SqliteOpenMode.ReadOnly));
    conexao.Open();

    using var comando = conexao.CreateCommand();
    comando.CommandText = "PRAGMA integrity_check;";
    var resultado = comando.ExecuteScalar()?.ToString();
    Console.WriteLine(resultado);

    return resultado == "ok" ? 0 : 1;
}

static void CopiarBanco(string origem, string destino)
{
    using var conexaoOrigem = new SqliteConnection(CriarConnectionString(origem, SqliteOpenMode.ReadOnly));
    using var conexaoDestino = new SqliteConnection(CriarConnectionString(destino, SqliteOpenMode.ReadWriteCreate));
    conexaoOrigem.Open();
    conexaoDestino.Open();
    conexaoOrigem.BackupDatabase(conexaoDestino);
}

static string CriarConnectionString(string caminhoBanco, SqliteOpenMode modo)
{
    return new SqliteConnectionStringBuilder
    {
        DataSource = caminhoBanco,
        Mode = modo
    }.ToString();
}
