using System;
using System.Data.SQLite;
using System.IO;

namespace PayrollConsole.Data
{
    public class Database : IDisposable
    {
        private readonly string _dbPath;
        private SQLiteConnection? _connection;

        public Database(string dbPath)
        {
            _dbPath = dbPath;
        }

        public SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                _connection.Open();
            }
            return _connection;
        }

        public void Initialize()
        {
            bool newDb = !File.Exists(_dbPath);

            var conn = GetConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Employees (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT UNIQUE,
                    Name TEXT NOT NULL,
                    Department TEXT,
                    BaseSalary REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Payrolls (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeId INTEGER,
                    Month INTEGER,
                    Year INTEGER,
                    HoursWorked REAL,
                    GrossSalary REAL,
                    AFP REAL,
                    ARS REAL,
                    ISR REAL,
                    NetSalary REAL,
                    FOREIGN KEY(EmployeeId) REFERENCES Employees(Id)
                );
            ";
            cmd.ExecuteNonQuery();

            if (newDb)
            {
                using var seed = conn.CreateCommand();
                seed.CommandText = @"
                    INSERT INTO Employees (Code, Name, Department, BaseSalary) VALUES
                    ('E001','María López','Contabilidad',25000),
                    ('E002','Juan Pérez','Operaciones',18000),
                    ('E003','Ana Gómez','RRHH',22000);
                ";
                seed.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}
