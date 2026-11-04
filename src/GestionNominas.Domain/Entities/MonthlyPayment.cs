using System;
using System.Collections.Generic;
using System.Data;
using GestionNominas.Persistence;

namespace GestionNominas.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio: MonthlyPayment (Nómina Mensual / Pago Mensual).
    /// Mapea la tabla 'MonthlyPayments' del esquema relacional.
    /// Almacena el resultado del cálculo y liquidación de salarios para un empleado en un año y mes:
    /// - Salario bruto devengado (proporcional con bonus de trienios aplicado).
    /// - Salario neto resultante tras la retención impositiva del país de la oficina.
    /// - Porcentaje de bonus por trienios devengado.
    /// Aplica estrictamente los principios de POO: la clave foránea payEmployee se modela
    /// como una referencia directa al objeto de dominio Employee, no como un tipo primitivo.
    /// </summary>
    public class MonthlyPayment
    {
        #region Propiedades de Dominio

        /// <summary>
        /// Identificador único autonumérico de la nómina (Clave Primaria, INT).
        /// </summary>
        public int IdMonthlyPayment { get; set; }

        /// <summary>
        /// Relación de Dominio POO: Empleado titular de la nómina.
        /// Corresponde a la clave foránea 'payEmployee' vinculada a la tabla Employees.
        /// </summary>
        public Employee Employee { get; set; } = new Employee();

        /// <summary>
        /// Año natural de devengo de la nómina (INT, ej. 2026).
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Mes natural de devengo de la nómina (INT, 1 a 12).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Salario mensual bruto calculado (DECIMAL(11,2)).
        /// Incluye la proporción de días trabajados en cada puesto y el bonus por antigüedad.
        /// </summary>
        public decimal SalaryGross { get; set; }

        /// <summary>
        /// Salario mensual neto resultante (DECIMAL(11,2)).
        /// Salario bruto menos la retención fiscal aplicable en el país de la oficina.
        /// </summary>
        public decimal SalaryNet { get; set; }

        /// <summary>
        /// Porcentaje de bonus por trienios aplicado en la nómina (DECIMAL(4,2)).
        /// </summary>
        public decimal BonusApplied { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío (por defecto). Requerido para enlaces de datos, serialización y mapeo.
        /// </summary>
        public MonthlyPayment()
        {
        }

        /// <summary>
        /// Constructor parametrizado para nuevas nóminas (sin identificador de base de datos).
        /// </summary>
        /// <param name="employee">Objeto Employee titular del pago.</param>
        /// <param name="year">Año de liquidación.</param>
        /// <param name="month">Mes de liquidación (1 a 12).</param>
        /// <param name="salaryGross">Salario bruto liquidado.</param>
        /// <param name="salaryNet">Salario neto a percibir.</param>
        /// <param name="bonusApplied">Porcentaje de bonus aplicado.</param>
        public MonthlyPayment(Employee employee, int year, int month, decimal salaryGross, decimal salaryNet, decimal bonusApplied)
        {
            Employee = employee ?? throw new ArgumentNullException(nameof(employee), "El empleado titular no puede ser nulo.");
            Year = year;
            Month = month;
            SalaryGross = salaryGross;
            SalaryNet = salaryNet;
            BonusApplied = bonusApplied;
        }

        /// <summary>
        /// Constructor parametrizado completo (para reconstrucción desde la persistencia).
        /// </summary>
        /// <param name="idMonthlyPayment">Identificador único de la nómina.</param>
        /// <param name="employee">Objeto Employee titular del pago.</param>
        /// <param name="year">Año de liquidación.</param>
        /// <param name="month">Mes de liquidación (1 a 12).</param>
        /// <param name="salaryGross">Salario bruto liquidado.</param>
        /// <param name="salaryNet">Salario neto a percibir.</param>
        /// <param name="bonusApplied">Porcentaje de bonus aplicado.</param>
        public MonthlyPayment(int idMonthlyPayment, Employee employee, int year, int month, decimal salaryGross, decimal salaryNet, decimal bonusApplied)
            : this(employee, year, month, salaryGross, salaryNet, bonusApplied)
        {
            IdMonthlyPayment = idMonthlyPayment;
        }

        #endregion

        #region Métodos de Persistencia (Patrón Database Broker)

        /// <summary>
        /// Inserta la nómina generada en la base de datos a través del Database Broker.
        /// </summary>
        public bool Insert()
        {
            Validate();

            string sql = @"INSERT INTO MonthlyPayments (payEmployee, payYear, payMonth, paySalaryGross, paySalaryNet, payBonusApplied) 
                           VALUES (@payEmployee, @payYear, @payMonth, @paySalaryGross, @paySalaryNet, @payBonusApplied);";

            var parameters = new Dictionary<string, object?>
            {
                { "@payEmployee", Employee.IdEmployee },
                { "@payYear", Year },
                { "@payMonth", Month },
                { "@paySalaryGross", SalaryGross },
                { "@paySalaryNet", SalaryNet },
                { "@payBonusApplied", BonusApplied }
            };

            long generatedId = DBBroker.GetInstance().ExecuteInsertAndGetId(sql, parameters);
            if (generatedId > 0)
            {
                IdMonthlyPayment = (int)generatedId;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Actualiza una nómina existente en la base de datos.
        /// </summary>
        public bool Update()
        {
            Validate();

            string sql = @"UPDATE MonthlyPayments 
                           SET payEmployee = @payEmployee, payYear = @payYear, payMonth = @payMonth, 
                               paySalaryGross = @paySalaryGross, paySalaryNet = @paySalaryNet, payBonusApplied = @payBonusApplied 
                           WHERE idMonthlyPayment = @idMonthlyPayment;";

            var parameters = new Dictionary<string, object?>
            {
                { "@idMonthlyPayment", IdMonthlyPayment },
                { "@payEmployee", Employee.IdEmployee },
                { "@payYear", Year },
                { "@payMonth", Month },
                { "@paySalaryGross", SalaryGross },
                { "@paySalaryNet", SalaryNet },
                { "@payBonusApplied", BonusApplied }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Elimina la nómina de la base de datos.
        /// </summary>
        public bool Delete()
        {
            string sql = "DELETE FROM MonthlyPayments WHERE idMonthlyPayment = @idMonthlyPayment;";
            var parameters = new Dictionary<string, object?>
            {
                { "@idMonthlyPayment", IdMonthlyPayment }
            };

            int affected = DBBroker.GetInstance().ExecuteNonQuery(sql, parameters);
            return affected > 0;
        }

        /// <summary>
        /// Lee y mapea todas las nóminas registradas cargando los objetos Employee y Country asociados.
        /// </summary>
        public static List<MonthlyPayment> ReadAll()
        {
            string sql = @"SELECT m.idMonthlyPayment, m.payEmployee, m.payYear, m.payMonth, 
                                  m.paySalaryGross, m.paySalaryNet, m.payBonusApplied,
                                  e.empName, e.empCountry, e.empIBAN,
                                  c.couName, c.couTaxBase
                           FROM MonthlyPayments m
                           INNER JOIN Employees e ON m.payEmployee = e.idEmployee
                           INNER JOIN Countries c ON e.empCountry = c.idCountry
                           ORDER BY m.payYear DESC, m.payMonth DESC, m.paySalaryGross DESC;";

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql);
            var list = new List<MonthlyPayment>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Obtiene todas las nóminas correspondientes a un año y mes concreto,
        /// opcionalmente filtradas por una oficina específica.
        /// Ordenadas de mayor sueldo bruto a menor.
        /// </summary>
        public static List<MonthlyPayment> ReadByPeriod(int year, int month, int? officeId = null)
        {
            string sql;
            var parameters = new Dictionary<string, object?>
            {
                { "@year", year },
                { "@month", month }
            };

            if (officeId.HasValue && officeId.Value > 0)
            {
                sql = @"SELECT DISTINCT m.idMonthlyPayment, m.payEmployee, m.payYear, m.payMonth, 
                                        m.paySalaryGross, m.paySalaryNet, m.payBonusApplied,
                                        e.empName, e.empCountry, e.empIBAN,
                                        c.couName, c.couTaxBase
                        FROM MonthlyPayments m
                        INNER JOIN Employees e ON m.payEmployee = e.idEmployee
                        INNER JOIN Countries c ON e.empCountry = c.idCountry
                        INNER JOIN Positions p ON p.posEmployee = e.idEmployee
                        WHERE m.payYear = @year AND m.payMonth = @month AND p.posOffice = @officeId
                        ORDER BY m.paySalaryGross DESC;";
                parameters.Add("@officeId", officeId.Value);
            }
            else
            {
                sql = @"SELECT m.idMonthlyPayment, m.payEmployee, m.payYear, m.payMonth, 
                               m.paySalaryGross, m.paySalaryNet, m.payBonusApplied,
                               e.empName, e.empCountry, e.empIBAN,
                               c.couName, c.couTaxBase
                        FROM MonthlyPayments m
                        INNER JOIN Employees e ON m.payEmployee = e.idEmployee
                        INNER JOIN Countries c ON e.empCountry = c.idCountry
                        WHERE m.payYear = @year AND m.payMonth = @month
                        ORDER BY m.paySalaryGross DESC;";
            }

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql, parameters);
            var list = new List<MonthlyPayment>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Obtiene todas las nóminas históricas de un empleado específico ordenadas de más reciente a más antigua.
        /// </summary>
        public static List<MonthlyPayment> ReadByEmployee(int employeeId)
        {
            string sql = @"SELECT m.idMonthlyPayment, m.payEmployee, m.payYear, m.payMonth, 
                                  m.paySalaryGross, m.paySalaryNet, m.payBonusApplied,
                                  e.empName, e.empCountry, e.empIBAN,
                                  c.couName, c.couTaxBase
                           FROM MonthlyPayments m
                           INNER JOIN Employees e ON m.payEmployee = e.idEmployee
                           INNER JOIN Countries c ON e.empCountry = c.idCountry
                           WHERE m.payEmployee = @employeeId
                           ORDER BY m.payYear DESC, m.payMonth DESC;";

            var parameters = new Dictionary<string, object?>
            {
                { "@employeeId", employeeId }
            };

            DataTable dt = DBBroker.GetInstance().ExecuteQuery(sql, parameters);
            var list = new List<MonthlyPayment>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapFromRow(row));
            }
            return list;
        }

        /// <summary>
        /// Comprueba si ya se han generado previamente nóminas para el mes y año dados.
        /// </summary>
        public static bool PeriodAlreadyGenerated(int year, int month)
        {
            string sql = "SELECT COUNT(*) FROM MonthlyPayments WHERE payYear = @year AND payMonth = @month;";
            var parameters = new Dictionary<string, object?>
            {
                { "@year", year },
                { "@month", month }
            };

            object? count = DBBroker.GetInstance().ExecuteScalar(sql, parameters);
            return count != null && Convert.ToInt32(count) > 0;
        }

        /// <summary>
        /// Mapea una fila de datos a un objeto MonthlyPayment instanciando el objeto Employee.
        /// </summary>
        private static MonthlyPayment MapFromRow(DataRow row)
        {
            var country = new Country
            {
                IdCountry = row["empCountry"].ToString() ?? string.Empty,
                Name = row.Table.Columns.Contains("couName") ? row["couName"].ToString() ?? string.Empty : string.Empty,
                TaxBase = row.Table.Columns.Contains("couTaxBase") ? Convert.ToDecimal(row["couTaxBase"]) : 0m
            };

            var employee = new Employee
            {
                IdEmployee = Convert.ToInt32(row["payEmployee"]),
                Name = row["empName"].ToString() ?? string.Empty,
                Country = country,
                IBAN = row["empIBAN"].ToString() ?? string.Empty
            };

            return new MonthlyPayment
            {
                IdMonthlyPayment = Convert.ToInt32(row["idMonthlyPayment"]),
                Employee = employee,
                Year = Convert.ToInt32(row["payYear"]),
                Month = Convert.ToInt32(row["payMonth"]),
                SalaryGross = Convert.ToDecimal(row["paySalaryGross"]),
                SalaryNet = Convert.ToDecimal(row["paySalaryNet"]),
                BonusApplied = Convert.ToDecimal(row["payBonusApplied"])
            };
        }

        #endregion

        #region Reglas de Validación de Negocio

        /// <summary>
        /// Valida la consistencia de los datos de la nómina.
        /// </summary>
        public void Validate()
        {
            if (Employee == null || Employee.IdEmployee <= 0)
            {
                throw new ArgumentException("La nómina debe estar asignada a un empleado válido.");
            }

            if (Year < 1900 || Year > 2100)
            {
                throw new ArgumentOutOfRangeException(nameof(Year), "El año especificado no es válido.");
            }

            if (Month < 1 || Month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(Month), "El mes debe estar comprendido entre 1 y 12.");
            }

            if (SalaryGross < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(SalaryGross), "El salario bruto no puede ser negativo.");
            }

            if (SalaryNet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(SalaryNet), "El salario neto no puede ser negativo.");
            }

            if (BonusApplied < 0 || BonusApplied > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(BonusApplied), "El bonus aplicado debe estar entre 0.00% y 100.00%.");
            }
        }

        #endregion

        public override string ToString()
        {
            return $"Nómina {Year}-{Month:D2}: {Employee.Name} - Bruto: {SalaryGross:C2} | Neto: {SalaryNet:C2}";
        }
    }
}
