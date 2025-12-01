using System;
using System.Collections.Generic;
using System.IO;
using PayrollConsole.Data;
using PayrollConsole.Models;

namespace PayrollConsole.Services
{
    public class PayrollSlip
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public decimal GrossSalary { get; set; }
        public decimal AFP { get; set; }
        public decimal ARS { get; set; }
        public decimal ISRAmount { get; set; }

        public decimal AFPAmount => Math.Round(GrossSalary * AFP, 2);
        public decimal ARSAmount => Math.Round(GrossSalary * ARS, 2);
        public decimal NetSalary => Math.Round(GrossSalary - AFPAmount - ARSAmount - ISRAmount, 2);
    }

    public class PayrollService
    {
        private readonly IEmployeeRepository _repo;
        private readonly Database _db;

        private readonly Dictionary<int, decimal> _hours = new();

        // Deducciones según el PDF (AFP 2.87%, ARS 3.04%)
        const decimal AFP_RATE = 0.0287m;
        const decimal ARS_RATE = 0.0304m;

        public PayrollService(IEmployeeRepository repo, Database db)
        {
            _repo = repo;
            _db = db;
        }

        public void RegisterHours(int employeeId, decimal hours)
        {
            _hours[employeeId] = hours;
        }

        public PayrollSlip? CalculatePayrollForEmployee(int employeeId)
        {
            var emp = _repo.GetById(employeeId);
            if (emp == null) return null;

            decimal hours = _hours.ContainsKey(employeeId) ? _hours[employeeId] : 160m;
            decimal hourlyValue = emp.BaseSalary / 160m;
            decimal gross = Math.Round(hourlyValue * hours, 2);

            var slip = new PayrollSlip
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name ?? "",
                GrossSalary = gross,
                AFP = AFP_RATE,
                ARS = ARS_RATE,
                ISRAmount = CalculateISR(gross)
            };

            SavePayrollRecord(
                emp.Id, gross, hours,
                slip.AFPAmount, slip.ARSAmount,
                slip.ISRAmount, slip.NetSalary
            );

            return slip;
        }

        private decimal CalculateISR(decimal gross)
        {
            // Simplificación: ISR solo aplicable si excede 34,685
            decimal threshold = 34685m;

            if (gross <= threshold) return 0m;

            decimal excess = gross - threshold;
            return Math.Round(excess * 0.15m, 2);
        }

        private void SavePayrollRecord(
            int empId, decimal gross, decimal hours,
            decimal afp, decimal ars, decimal isr, decimal net)
        {
            using var cmd = _db.GetConnection().CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Payrolls 
                (EmployeeId, Month, Year, HoursWorked, GrossSalary, AFP, ARS, ISR, NetSalary)
                VALUES (@eid,@m,@y,@h,@g,@a,@r,@i,@n)";
            
            cmd.Parameters.AddWithValue("@eid", empId);
            cmd.Parameters.AddWithValue("@m", DateTime.Now.Month);
            cmd.Parameters.AddWithValue("@y", DateTime.Now.Year);
            cmd.Parameters.AddWithValue("@h", hours);
            cmd.Parameters.AddWithValue("@g", gross);
            cmd.Parameters.AddWithValue("@a", afp);
            cmd.Parameters.AddWithValue("@r", ars);
            cmd.Parameters.AddWithValue("@i", isr);
            cmd.Parameters.AddWithValue("@n", net);

            cmd.ExecuteNonQuery();
        }

        public void GenerateMonthlyReport(string path)
        {
            var lines = new List<string>
            {
                "Empleado,Salario Bruto,AFP,ARS,ISR,Salario Neto"
            };

            foreach (var e in _repo.GetAll())
            {
                decimal hours = _hours.ContainsKey(e.Id) ? _hours[e.Id] : 160m;
                decimal hourlyVal = e.BaseSalary / 160m;
                decimal gross = Math.Round(hourlyVal * hours, 2);

                decimal afp = Math.Round(gross * AFP_RATE, 2);
                decimal ars = Math.Round(gross * ARS_RATE, 2);
                decimal isr = CalculateISR(gross);
                decimal net = Math.Round(gross - afp - ars - isr, 2);

                lines.Add($"{e.Name},{gross},{afp},{ars},{isr},{net}");
            }

            File.WriteAllLines(path, lines);
        }
    }
}
