using MySqlConnector;
using System.IO;

var connectionString = "Server=localhost;Uid=root;Pwd=mayank;";
var sql = File.ReadAllText(@"..\LMSystem\mysql_seed.sql");

// split by ; to get separate statements
var commands = sql.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

using var conn = new MySqlConnection(connectionString);
conn.Open();
foreach(var cmdText in commands) {
    if (string.IsNullOrWhiteSpace(cmdText)) continue;
    using var c = new MySqlCommand(cmdText, conn);
    c.ExecuteNonQuery();
}
Console.WriteLine("Seed script executed successfully.");
