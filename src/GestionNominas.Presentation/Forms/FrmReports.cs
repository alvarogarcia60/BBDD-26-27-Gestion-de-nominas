using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestionNominas.Domain.Entities;
using GestionNominas.Domain.Services;

namespace GestionNominas.Presentation.Forms
{
    /// <summary>
    /// Módulo de Informes. Agrupa los 5 informes exigidos en el enunciado
    /// en un TabControl con filtros interactivos.
    /// </summary>
    public class FrmReports : Form
    {
        private TabControl tabReports = null!;

        // ── Reporte 1: Nóminas de un mes ordenadas de mayor a menor bruto ──
        private NumericUpDown numR1Year  = null!;
        private ComboBox      cmbR1Month = null!;
        private ComboBox      cmbR1Office = null!;
        private Button        btnR1Filter = null!;
        private DataGridView  dgvR1 = null!;
        private Label         lblR1Total = null!;

        // ── Reporte 2: Histórico nóminas de un empleado ──
        private ComboBox     cmbR2Employee = null!;
        private Button       btnR2Filter   = null!;
        private DataGridView dgvR2 = null!;
        private Label        lblR2Info = null!;

        // ── Reporte 3: Empleados por sede y trienios consolidados ──
        private Button       btnR3Load = null!;
        private DataGridView dgvR3 = null!;

        // ── Reporte 4: Gasto en nóminas por oficina en un mes ──
        private NumericUpDown numR4Year   = null!;
        private ComboBox      cmbR4Month  = null!;
        private Button        btnR4Load   = null!;
        private DataGridView  dgvR4 = null!;
        private Label         lblR4Total  = null!;

        // ── Reporte 5: Gasto en nóminas por país a lo largo de los meses ──
        private Button       btnR5Load = null!;
        private DataGridView dgvR5 = null!;

        // ────────────────────────────────────────────────────────────────────

        public FrmReports()
        {
            InitializeComponentManual();
            LoadInitialData();
        }

        // ────────────────────────────────────────────────────────────────────
        // Construcción de la interfaz
        // ────────────────────────────────────────────────────────────────────

        private void InitializeComponentManual()
        {
            Text            = "Módulo de Informes y Estadísticas de Nóminas";
            Size            = new Size(1050, 680);
            StartPosition   = FormStartPosition.CenterParent;
            Font            = new Font("Segoe UI", 9F);
            MinimumSize     = new Size(900, 600);

            tabReports = new TabControl { Dock = DockStyle.Fill };

            var tab1 = new TabPage("1. Nóminas Mes / Oficina");
            var tab2 = new TabPage("2. Historial Empleado");
            var tab3 = new TabPage("3. Empleados y Trienios por Sede");
            var tab4 = new TabPage("4. Gasto Oficina / Mes");
            var tab5 = new TabPage("5. Gasto País / Mes Histórico");

            BuildTab1(tab1);
            BuildTab2(tab2);
            BuildTab3(tab3);
            BuildTab4(tab4);
            BuildTab5(tab5);

            tabReports.TabPages.AddRange(new[] { tab1, tab2, tab3, tab4, tab5 });
            Controls.Add(tabReports);
        }

        // ── Tab 1 ─────────────────────────────────────────────────────────
        private void BuildTab1(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10, 8, 10, 0) };

            pnl.Controls.Add(new Label { Text = "Año:", Location = new Point(15, 18), AutoSize = true });
            numR1Year = new NumericUpDown
            {
                Location = new Point(50, 15), Width = 80,
                Minimum = 2000, Maximum = 2100, Value = DateTime.Today.AddMonths(-1).Year
            };
            pnl.Controls.Add(numR1Year);

            pnl.Controls.Add(new Label { Text = "Mes:", Location = new Point(145, 18), AutoSize = true });
            cmbR1Month = new ComboBox { Location = new Point(182, 15), Width = 105, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 12; i++) cmbR1Month.Items.Add(i.ToString("D2"));
            cmbR1Month.SelectedIndex = DateTime.Today.AddMonths(-1).Month - 1;
            pnl.Controls.Add(cmbR1Month);

            pnl.Controls.Add(new Label { Text = "Oficina:", Location = new Point(300, 18), AutoSize = true });
            cmbR1Office = new ComboBox { Location = new Point(352, 15), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            pnl.Controls.Add(cmbR1Office);

            btnR1Filter = new Button
            {
                Text = "Filtrar", Location = new Point(665, 13), Width = 100, Height = 28,
                BackColor = Color.SteelBlue, ForeColor = Color.White
            };
            btnR1Filter.Click += (s, e) => LoadReport1();
            pnl.Controls.Add(btnR1Filter);

            lblR1Total = new Label
            {
                Location = new Point(15, 46), AutoSize = true, ForeColor = Color.DarkSlateGray,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            pnl.Controls.Add(lblR1Total);

            dgvR1 = BuildGrid();
            tab.Controls.Add(dgvR1);
            tab.Controls.Add(pnl);
        }

        // ── Tab 2 ─────────────────────────────────────────────────────────
        private void BuildTab2(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10, 8, 10, 0) };

            pnl.Controls.Add(new Label { Text = "Empleado:", Location = new Point(15, 18), AutoSize = true });
            cmbR2Employee = new ComboBox { Location = new Point(90, 15), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
            pnl.Controls.Add(cmbR2Employee);

            btnR2Filter = new Button
            {
                Text = "Ver Historial", Location = new Point(488, 13), Width = 130, Height = 28,
                BackColor = Color.SteelBlue, ForeColor = Color.White
            };
            btnR2Filter.Click += (s, e) => LoadReport2();
            pnl.Controls.Add(btnR2Filter);

            lblR2Info = new Label
            {
                Location = new Point(15, 46), AutoSize = true, ForeColor = Color.DarkSlateGray,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            pnl.Controls.Add(lblR2Info);

            dgvR2 = BuildGrid();
            tab.Controls.Add(dgvR2);
            tab.Controls.Add(pnl);
        }

        // ── Tab 3 ─────────────────────────────────────────────────────────
        private void BuildTab3(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10, 10, 10, 0) };
            btnR3Load = new Button
            {
                Text = "Cargar: Empleados y Trienios por Sede",
                Location = new Point(15, 12), Width = 320, Height = 32,
                BackColor = Color.SteelBlue, ForeColor = Color.White
            };
            btnR3Load.Click += (s, e) => LoadReport3();
            pnl.Controls.Add(btnR3Load);

            pnl.Controls.Add(new Label
            {
                Text = "Muestra todos los empleados de cada sede con sus trienios históricos acumulados hasta hoy.",
                Location = new Point(350, 18), AutoSize = true, ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            });

            dgvR3 = BuildGrid();
            tab.Controls.Add(dgvR3);
            tab.Controls.Add(pnl);
        }

        // ── Tab 4 ─────────────────────────────────────────────────────────
        private void BuildTab4(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10, 8, 10, 0) };

            pnl.Controls.Add(new Label { Text = "Año:", Location = new Point(15, 18), AutoSize = true });
            numR4Year = new NumericUpDown
            {
                Location = new Point(50, 15), Width = 80,
                Minimum = 2000, Maximum = 2100, Value = DateTime.Today.AddMonths(-1).Year
            };
            pnl.Controls.Add(numR4Year);

            pnl.Controls.Add(new Label { Text = "Mes:", Location = new Point(145, 18), AutoSize = true });
            cmbR4Month = new ComboBox { Location = new Point(182, 15), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 12; i++) cmbR4Month.Items.Add(i.ToString("D2"));
            cmbR4Month.SelectedIndex = DateTime.Today.AddMonths(-1).Month - 1;
            pnl.Controls.Add(cmbR4Month);

            btnR4Load = new Button
            {
                Text = "Calcular Gasto por Oficina", Location = new Point(290, 13), Width = 200, Height = 28,
                BackColor = Color.SteelBlue, ForeColor = Color.White
            };
            btnR4Load.Click += (s, e) => LoadReport4();
            pnl.Controls.Add(btnR4Load);

            lblR4Total = new Label
            {
                Location = new Point(15, 46), AutoSize = true, ForeColor = Color.DarkSlateGray,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            pnl.Controls.Add(lblR4Total);

            dgvR4 = BuildGrid();
            tab.Controls.Add(dgvR4);
            tab.Controls.Add(pnl);
        }

        // ── Tab 5 ─────────────────────────────────────────────────────────
        private void BuildTab5(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10, 10, 10, 0) };
            btnR5Load = new Button
            {
                Text = "Generar Informe: Gasto por País y Mes",
                Location = new Point(15, 12), Width = 300, Height = 32,
                BackColor = Color.SteelBlue, ForeColor = Color.White
            };
            btnR5Load.Click += (s, e) => LoadReport5();
            pnl.Controls.Add(btnR5Load);

            pnl.Controls.Add(new Label
            {
                Text = "Gasto total en nóminas por país, desglosado por cada periodo (año-mes) disponible.",
                Location = new Point(330, 18), AutoSize = true, ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            });

            dgvR5 = BuildGrid();
            tab.Controls.Add(dgvR5);
            tab.Controls.Add(pnl);
        }

        // ── Factory para DataGridViews ─────────────────────────────────────
        private static DataGridView BuildGrid() =>
            new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                MultiSelect         = false,
                BackgroundColor     = Color.White,
                GridColor           = Color.LightGray,
                BorderStyle         = BorderStyle.None,
                RowHeadersWidth     = 30
            };

        // ────────────────────────────────────────────────────────────────────
        // Carga inicial de combos
        // ────────────────────────────────────────────────────────────────────

        private void LoadInitialData()
        {
            try
            {
                // Combo de oficinas para Reporte 1 (con opción "Todas")
                var offices = Office.ReadAll();
                cmbR1Office.Items.Clear();
                cmbR1Office.Items.Add("— Todas las oficinas —");
                foreach (var off in offices)
                    cmbR1Office.Items.Add(off);
                cmbR1Office.SelectedIndex = 0;

                // Combo de empleados para Reporte 2
                var employees = Employee.ReadAll();
                cmbR2Employee.DataSource    = employees;
                cmbR2Employee.DisplayMember = "Name";
                cmbR2Employee.ValueMember   = "IdEmployee";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar filtros de informes:\n{ex.Message}",
                    "Error de Inicialización", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Informe 1: Nóminas del mes ordenadas de mayor a menor sueldo bruto
        //            con filtro opcional por oficina
        // ────────────────────────────────────────────────────────────────────

        private void LoadReport1()
        {
            try
            {
                int  year    = (int)numR1Year.Value;
                int  month   = cmbR1Month.SelectedIndex + 1;
                int? officeId = null;

                if (cmbR1Office.SelectedIndex > 0 && cmbR1Office.SelectedItem is Office off)
                    officeId = off.IdOffice;

                // Delega en el método de dominio que ya aplica ORDER BY paySalaryGross DESC
                var list = MonthlyPayment.ReadByPeriod(year, month, officeId);

                // Proyección de columnas legibles para el DataGridView
                var table = new DataTable();
                table.Columns.Add("ID",              typeof(int));
                table.Columns.Add("Empleado",        typeof(string));
                table.Columns.Add("Año",             typeof(int));
                table.Columns.Add("Mes",             typeof(int));
                table.Columns.Add("Bruto (€)",       typeof(decimal));
                table.Columns.Add("Neto (€)",        typeof(decimal));
                table.Columns.Add("Bonus Aplicado %",typeof(decimal));

                foreach (var p in list)
                    table.Rows.Add(p.IdMonthlyPayment, p.Employee.Name,
                                   p.Year, p.Month,
                                   p.SalaryGross, p.SalaryNet, p.BonusApplied);

                dgvR1.DataSource = table;
                FormatMoneyColumns(dgvR1, "Bruto (€)", "Neto (€)");

                decimal totalGross = list.Sum(p => p.SalaryGross);
                decimal totalNet   = list.Sum(p => p.SalaryNet);
                lblR1Total.Text    =
                    $"{list.Count} nóminas  |  Total Bruto: {totalGross:N2} €  |  Total Neto: {totalNet:N2} €";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Informe 1:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Informe 2: Histórico de nóminas de un empleado
        // ────────────────────────────────────────────────────────────────────

        private void LoadReport2()
        {
            try
            {
                if (cmbR2Employee.SelectedValue is not int empId) return;

                var list = MonthlyPayment.ReadByEmployee(empId);

                var table = new DataTable();
                table.Columns.Add("Año",             typeof(int));
                table.Columns.Add("Mes",             typeof(int));
                table.Columns.Add("Bruto (€)",       typeof(decimal));
                table.Columns.Add("Neto (€)",        typeof(decimal));
                table.Columns.Add("Bonus Aplicado %",typeof(decimal));

                foreach (var p in list)
                    table.Rows.Add(p.Year, p.Month, p.SalaryGross, p.SalaryNet, p.BonusApplied);

                dgvR2.DataSource = table;
                FormatMoneyColumns(dgvR2, "Bruto (€)", "Neto (€)");

                decimal acumGross = list.Sum(p => p.SalaryGross);
                decimal acumNet   = list.Sum(p => p.SalaryNet);
                lblR2Info.Text    =
                    $"{list.Count} nóminas históricas  |  Acumulado Bruto: {acumGross:N2} €  |  Acumulado Neto: {acumNet:N2} €";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Informe 2:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Informe 3: Empleados de cada sede con los trienios acumulados hasta hoy
        // ────────────────────────────────────────────────────────────────────

        private void LoadReport3()
        {
            try
            {
                dgvR3.DataSource = ReportService.GetEmployeesWithTrieniosByOffice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Informe 3:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Informe 4: Gasto total en nóminas de cada oficina en un mes
        // ────────────────────────────────────────────────────────────────────

        private void LoadReport4()
        {
            try
            {
                int year  = (int)numR4Year.Value;
                int month = cmbR4Month.SelectedIndex + 1;

                DataTable dt = ReportService.GetOfficeExpensesByMonth(year, month);
                dgvR4.DataSource = dt;
                FormatMoneyColumns(dgvR4, "Gasto Bruto Total (€)", "Gasto Neto Total (€)");

                decimal grandTotal = 0;
                foreach (DataRow row in dt.Rows)
                    grandTotal += Convert.ToDecimal(row["Gasto Bruto Total (€)"]);

                lblR4Total.Text = dt.Rows.Count > 0
                    ? $"Gasto bruto total del periodo {year}-{month:D2}: {grandTotal:N2} €"
                    : $"No hay nóminas registradas para el periodo {year}-{month:D2}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Informe 4:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Informe 5: Gasto en nóminas por país en cada mes con datos
        // ────────────────────────────────────────────────────────────────────

        private void LoadReport5()
        {
            try
            {
                dgvR5.DataSource = ReportService.GetCountryExpensesByMonth();
                FormatMoneyColumns(dgvR5, "Gasto Bruto (€)", "Gasto Neto (€)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Informe 5:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Utilidades de formato visual
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Aplica formato monetario N2 a las columnas indicadas de un DataGridView.
        /// </summary>
        private static void FormatMoneyColumns(DataGridView dgv, params string[] columnNames)
        {
            foreach (string colName in columnNames)
            {
                if (dgv.Columns.Contains(colName))
                {
                    dgv.Columns[colName].DefaultCellStyle.Format    = "N2";
                    dgv.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }
    }
}
