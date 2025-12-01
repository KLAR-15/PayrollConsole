using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PayrollConsole.Models;

namespace PayrollConsole.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly Database _db;
        public EmployeeRepository(Database db)
        {
            _db = db;
        }

        public bool Add(Employee e)
        {
            if (string.IsNullOrWhiteSpace(e.Name) || e.BaseSalary < 0)
                return false;

            if (!string.IsNullOrWhiteSpace(e.Code) && GetByCode(e.Code!) != null)
                return false;

            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = @"INSERT INTO Employees (Code, Name, Department, BaseSalary)
                                VALUES (@code, @name, @dept, @salary)";
            cmd.Parameters.AddWithValue("@code", e.Code ?? "");
            cmd.Parameters.AddWithValue("@name", e.Name ?? "");
            cmd.Parameters.AddWithValue("@dept", e.Department ?? "");
            cmd.Parameters.AddWithValue("@salary", e.BaseSalary);

            return cmd.ExecuteNonQuery() > 0;
        }

        public IEnumerable<Employee> GetAll()
        {
            var list = new List<Employee>();

            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = "SELECT Id, Code, Name, Department, BaseSalary FROM Employees";

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new Employee
                {
                    Id = rdr.GetInt32(0),
                    Code = rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                    Name = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                    Department = rdr.IsDBNull(3) ? "" : rdr.GetString(3),
                    BaseSalary = (decimal)rdr.GetDouble(4)
                });
            }

            return list;
        }

        public Employee? GetById(int id)
        {
            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = @"SELECT Id, Code, Name, Department, BaseSalary
                                FROM Employees WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                return new Employee
                {
                    Id = rdr.GetInt32(0),
                    Code = rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                    Name = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                    Department = rdr.IsDBNull(3) ? "" : rdr.GetString(3),
                    BaseSalary = (decimal)rdr.GetDouble(4)
                };
            }

            return null;
        }

        public Employee? GetByCode(string code)
        {
            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = @"SELECT Id, Code, Name, Department, BaseSalary
                                FROM Employees WHERE Code = @code";
            cmd.Parameters.AddWithValue("@code", code);

            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                return new Employee
                {
                    Id = rdr.GetInt32(0),
                    Code = rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                    Name = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                    Department = rdr.IsDBNull(3) ? "" : rdr.GetString(3),
                    BaseSalary = (decimal)rdr.GetDouble(4)
                };
            }

            return null;
        }

        public bool Update(Employee e)
        {
            var existing = GetByCode(e.Code ?? "");
            if (existing != null && existing.Id != e.Id)
                return false;

            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = @"UPDATE Employees SET 
                                Code=@code, Name=@name, Department=@dept, BaseSalary=@salary 
                                WHERE Id=@id";

            cmd.Parameters.AddWithValue("@code", e.Code ?? "");
            cmd.Parameters.AddWithValue("@name", e.Name ?? "");
            cmd.Parameters.AddWithValue("@dept", e.Department ?? "");
            cmd.Parameters.AddWithValue("@salary", e.BaseSalary);
            cmd.Parameters.AddWithValue("@id", e.Id);

            return cmd.ExecuteNonQuery() > 0;
        }

        public void Delete(int id)
        {
            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = "DELETE FROM Employees WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
