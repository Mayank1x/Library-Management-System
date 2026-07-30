using System;
using System.IO;
using MySqlConnector;

class Program
{
    static void Main()
    {
        string connectionString = "Server=localhost;User ID=root;Password=mayank;";
        
        try
        {
            Console.WriteLine("Connecting to MySQL...");
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("Connected!");

            string sqlFilePath = Path.Combine(AppContext.BaseDirectory, "../../../..", "LMSystem", "module7_seed.sql");
            if (!File.Exists(sqlFilePath))
            {
                Console.WriteLine($"Could not find {sqlFilePath}");
                return;
            }

            Console.WriteLine("Reading SQL script...");
            string script = File.ReadAllText(sqlFilePath);
            
            using var command = new MySqlCommand(script, connection);
            command.ExecuteNonQuery();
            
            Console.WriteLine("Database setup successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
