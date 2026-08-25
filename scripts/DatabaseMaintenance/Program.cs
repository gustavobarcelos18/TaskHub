using Microsoft.Data.Sqlite;
if ((args.Length != 3 || (args[0] != "backup" && args[0] != "restore")) &&
    (args.Length != 2 || args[0] != "integrity-check"))
{
    Console.Error.WriteLine("Uso: backup <banco> <diretorio> | restore <backup> <banco> | integrity-check <banco>");
    return 1;
}

if (args[0] == "integrity-check")
{
    var databasePath = Path.GetFullPath(args[1]);

    if (!File.Exists(databasePath)) { Console.Error.WriteLine($"Arquivo não encontrado: {databasePath}"); return 1; }

    using var connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadOnly");
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = "PRAGMA integrity_check;";
    var result = command.ExecuteScalar()?.ToString();
    Console.WriteLine(result);
    return result == "ok" ? 0 : 1;
}

var source = Path.GetFullPath(args[1]); var target = args[0] == "backup" ? Path.Combine(Path.GetFullPath(args[2]), $"tarefas-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db") : Path.GetFullPath(args[2]);
if (!File.Exists(source)) { Console.Error.WriteLine($"Arquivo não encontrado: {source}"); return 1; }
Directory.CreateDirectory(Path.GetDirectoryName(target)!);
if (args[0] == "restore" && File.Exists(target)) { var saved = Path.Combine(Path.GetDirectoryName(target)!, $"tarefas-pre-restore-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db"); Copy(target, saved); Console.WriteLine($"Banco anterior preservado em: {saved}"); }
Copy(source, target); Console.WriteLine(args[0] == "backup" ? $"Backup criado: {target}" : $"Backup restaurado em: {target}"); return 0;
static void Copy(string sourcePath, string targetPath) { using var source = new SqliteConnection($"Data Source={sourcePath};Mode=ReadOnly"); using var target = new SqliteConnection($"Data Source={targetPath};Mode=ReadWriteCreate"); source.Open(); target.Open(); source.BackupDatabase(target); }
