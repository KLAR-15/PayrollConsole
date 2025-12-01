using System;
using PayrollConsole.Data;
using PayrollConsole.Models;
using PayrollConsole.Services;

namespace PayrollConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Servicios Corporativos Caribe SRL - Prototype Nómina (Consola)");

            using var db = new Database("payroll.db");
            db.Initialize();

            var repo = new EmployeeRepository(db);
            var payroll = new PayrollService(repo, db);

            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n--- MENÚ PRINCIPAL ---");
                Console.WriteLine("1. Agregar empleado");
                Console.WriteLine("2. Listar empleados");
                Console.WriteLine("3. Editar empleado");
                Console.WriteLine("4. Eliminar empleado");
                Console.WriteLine("5. Registrar horas y calcular nómina");
                Console.WriteLine("6. Generar reporte mensual (CSV)");
                Console.WriteLine("7. Salir");
                Console.Write("Seleccione una opción: ");

                string option = Console.ReadLine() ?? "";

                switch (option)
                {
                    case "1":
                        AddEmployee(repo);
                        break;
                    case "2":
                        ListEmployees(repo);
                        break;
                    case "3":
                        EditEmployee(repo);
                        break;
                    case "4":
                        DeleteEmployee(repo);
                        break;
                    case "5":
                        RegisterHoursAndCalculate(repo, payroll);
                        break;
                    case "6":
                        payroll.GenerateMonthlyReport("monthly_report.csv");
                        Console.WriteLine("Reporte generado: monthly_report.csv");
                        break;
                    case "7":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }
        }

        static void AddEmployee(IEmployeeRepository repo)
        {
            Console.Write("Código: ");
            var code = Console.ReadLine()?.Trim();

            Console.Write("Nombre: ");
            var name = Console.ReadLine()?.Trim();

            Console.Write("Departamento: ");
            var dept = Console.ReadLine()?.Trim();

            Console.Write("Salario base: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal salary))
            {
                Console.WriteLine("Salario inválido.");
                return;
            }

            var emp = new Employee
            {
                Code = code,
                Name = name,
                Department = dept,
                BaseSalary = salary
            };

            bool result = repo.Add(emp);

            Console.WriteLine(result ? "Empleado agregado correctamente." : "Error: Datos inválidos o código duplicado.");
        }

        static void ListEmployees(IEmployeeRepository repo)
        {
            var employees = repo.GetAll();

            Console.WriteLine("\n--- LISTA DE EMPLEADOS ---");
            foreach (var e in employees)
            {
                Console.WriteLine($"{e.Id} | {e.Code} | {e.Name} | {e.Department} | {e.BaseSalary:C}");
            }
        }

        static void EditEmployee(IEmployeeRepository repo)
        {
            Console.Write("ID del empleado a editar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            var emp = repo.GetById(id);
            if (emp == null)
            {
                Console.WriteLine("Empleado no encontrado.");
                return;
            }

            Console.Write($"Nuevo código ({emp.Code}): ");
            var newCode = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newCode))
                emp.Code = newCode.Trim();

            Console.Write($"Nuevo nombre ({emp.Name}): ");
            var newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
                emp.Name = newName.Trim();

            Console.Write($"Nuevo departamento ({emp.Department}): ");
            var newDept = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newDept))
                emp.Department = newDept.Trim();

            Console.Write($"Nuevo salario base ({emp.BaseSalary}): ");
            var newSalaryInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newSalaryInput) && decimal.TryParse(newSalaryInput, out decimal newSalary))
                emp.BaseSalary = newSalary;

            bool updated = repo.Update(emp);

            Console.WriteLine(updated ? "Empleado actualizado." : "Error: código duplicado o datos inválidos.");
        }

        static void DeleteEmployee(IEmployeeRepository repo)
        {
            Console.Write("ID del empleado a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            repo.Delete(id);
            Console.WriteLine("Empleado eliminado (si existía).");
        }

        static void RegisterHoursAndCalculate(IEmployeeRepository repo, PayrollService payroll)
        {
            Console.Write("ID del empleado: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            var emp = repo.GetById(id);
            if (emp == null)
            {
                Console.WriteLine("Empleado no encontrado.");
                return;
            }

            Console.Write("Horas trabajadas este mes: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal hours))
            {
                Console.WriteLine("Horas inválidas.");
                return;
            }

            payroll.RegisterHours(id, hours);

            var slip = payroll.CalculatePayrollForEmployee(id);
            if (slip == null)
            {
                Console.WriteLine("No se pudo calcular la nómina.");
                return;
            }

            Console.WriteLine("\n--- RECIBO DE PAGO ---");
            Console.WriteLine($"Empleado: {slip.EmployeeName}");
            Console.WriteLine($"Salario Bruto: {slip.GrossSalary:C}");
            Console.WriteLine($"AFP: {slip.AFPAmount:C}");
            Console.WriteLine($"ARS: {slip.ARSAmount:C}");
            Console.WriteLine($"ISR: {slip.ISRAmount:C}");
            Console.WriteLine($"Salario Neto: {slip.NetSalary:C}");
        }
    }
}
