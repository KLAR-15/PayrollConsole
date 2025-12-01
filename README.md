# PayrollConsole – Sistema de Nómina en C# (.NET + SQLite)

**Proyecto Final – INF-512**

## 🔧 Descripción
PayrollConsole es una aplicación de consola en C# que permite gestionar empleados y generar nóminas mensuales con deducciones dominicanas:  
- AFP 2.87%  
- ARS 3.04%  
- ISR según cálculo simple  

Funcionalidades:
- CRUD completo de empleados (agregar, listar, editar, eliminar).  
- Registro de horas trabajadas + cálculo de salario bruto, deducciones y salario neto.  
- Persistencia en SQLite (base de datos `payroll.db`).  
- Patrón Repositorio (separación de lógica, datos y presentación).  
- Generación automática de reporte mensual en `monthly_report.csv`.  

## 🚀 Cómo usar

1. Clona este repositorio:  
   ```bash
   git clone https://github.com/KLAR-15/PayrollConsole.git
   cd PayrollConsole
