using System.Collections.Generic;
using PayrollConsole.Models;

namespace PayrollConsole.Data
{
    public interface IEmployeeRepository
    {
        bool Add(Employee e);
        IEnumerable<Employee> GetAll();
        Employee? GetById(int id);
        Employee? GetByCode(string code);
        bool Update(Employee e);
        void Delete(int id);
    }
}
