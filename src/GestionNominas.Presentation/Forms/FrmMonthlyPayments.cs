using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestionNominas.Domain.Entities;
using GestionNominas.Domain.Services;

namespace GestionNominas.Presentation.Forms
{
    /// <summary>
    /// Formulario de Generación y Consulta de Nóminas Mensuales.
    /// Invoca <see cref="PayrollCalculator.GenerateMonthlyPayroll"/> y muestra el resultado.
    /// </summary>
    public class FrmMonthlyPayments : Form
    {
        private NumericUpDown numYear        = null!;
        private ComboBox      cmbMonth       = null!;
        private Button        btnGenerate    = null!;
        private Button        btnConsult     = null!;
        private Button        btnDeletePeriod = null!;
        private DataGridView  dgvPayments    = null!;
        private Label         lblSummary     = null!;
        private Panel         panelTop       = null!;

        // ────────────────────────────────────────────────────────────────────

        public FrmMonthlyPayments()
        {
            InitializeComponentManual();
            SetDefaultPeriod();
        }

        // ────────────────────────────────────────────────────────────────────
        // Construcción de la interfaz
        // ────────────────────────────────────────────────────────────────────

        private void InitializeComponentManual()
        {
            Text          = "Generación y Consulta de Nóminas Mensuales";
            Size          = new Size(1000, 620);
            StartPosition = FormStartPosition.CenterParent;
            Font          = new Font("Segoe UI", 9F);
            MinimumSize   = new Size(800, 500);

            panelTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 120,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding   = new Padding(15, 12, 15, 0)
            };

            // ── Selectores de periodo ──────────────────────────────────────
            var lblYear = new Label
            {
                Text = "Año:", Location = new Point(15, 22), AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F)
            };
            numYear = new NumericUpDown
            {
                Location = new Point(55, 18), Width = 90,
                Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year
            };

            var lblMonth = new Label
            {
                Text = "Mes:", Location = new Point(160, 22), AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F)
            };
            cmbMonth = new ComboBox
            {
                Location = new Point(200, 18), Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMonth.Items.AddRange(new object[]
            {
                "01 - Enero",     "02 - Febrero",   "03 - Marzo",    "04 - Abril",
                "05 - Mayo",      "06 - Junio",     "07 - Julio",    "08 - Agosto",
                "09 - Septiembre","10 - Octubre",   "11 - Noviembre","12 - Diciembre"
            });

            // ── Botones ───────────────────────────────────────────────────
            btnGenerate = new Button
            {
                Text      = "⚡  Generar Nóminas del Periodo",
                Location  = new Point(380, 14),
                Width     = 250, Height = 36,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI Bold", 9.5F)
            };
            btnGenerate.Click += BtnGenerate_Click;

            btnConsult = new Button
            {
                Text      = "🔍  Consultar Periodo",
                Location  = new Point(645, 14),
                Width     = 170, Height = 36,
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White
            };
            btnConsult.Click += (s, e) => LoadPayments();

            btnDeletePeriod = new Button
            {
                Text      = "🗑  Eliminar Nóminas del Periodo",
                Location  = new Point(380, 58),
                Width     = 250, Height = 32,
                BackColor = Color.FromArgb(185, 28, 28),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9F)
            };
            btnDeletePeriod.Click += BtnDeletePeriod_Click;

            // ── Etiqueta de resumen ───────────────────────────────────────
            lblSummary = new Label
            {
                Text      = "Seleccione un mes anterior al mes actual para generar o consultar las nóminas.",
                Location  = new Point(15, 92),
                AutoSize  = true,
                ForeColor = Color.DarkSlateGray,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };

            panelTop.Controls.AddRange(new Control[]
            {
                lblYear, numYear, lblMonth, cmbMonth,
                btnGenerate, btnConsult, btnDeletePeriod, lblSummary
            });

            // ── DataGridView ──────────────────────────────────────────────
            dgvPayments = new DataGridView
            {
                Dock                = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                MultiSelect         = false,
                BackgroundColor     = Color.White,
                GridColor           = Color.LightGray,
                BorderStyle         = BorderStyle.None
            };

            Controls.Add(dgvPayments);
            Controls.Add(panelTop);
        }

        // ────────────────────────────────────────────────────────────────────
        // Inicialización
        // ────────────────────────────────────────────────────────────────────

        private void SetDefaultPeriod()
        {
            // Por defecto: mes anterior al actual
            DateTime prev = DateTime.Today.AddMonths(-1);
            numYear.Value           = prev.Year;
            cmbMonth.SelectedIndex  = prev.Month - 1;
        }

        // ────────────────────────────────────────────────────────────────────
        // Generar nóminas
        // ────────────────────────────────────────────────────────────────────

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            int year  = (int)numYear.Value;
            int month = cmbMonth.SelectedIndex + 1;

            // Confirmación antes de lanzar el proceso masivo
            var confirm = MessageBox.Show(
                $"¿Desea generar las nóminas para el periodo {year}-{month:D2}?\n\n" +
                "Esta acción insertará un registro de nómina por cada empleado activo en ese mes. " +
                "Si el periodo ya fue generado, la operación será rechazada.",
                "Confirmar Generación de Nóminas",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                var calculator = new PayrollCalculator();
                var result     = calculator.GenerateMonthlyPayroll(year, month);

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Proceso Finalizado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdateSummary(year, month, result.GeneratedPayments.Count,
                                  result.GeneratedPayments.Sum(p => p.SalaryGross),
                                  result.GeneratedPayments.Sum(p => p.SalaryNet));
                    LoadPayments();
                }
                else
                {
                    // PayrollResult.Success = false → problema no crítico capturado dentro del servicio
                    MessageBox.Show(result.Message, "Aviso de Generación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Parámetro de mes o año inválido
                MessageBox.Show(
                    $"Parámetro incorrecto: {ex.Message}",
                    "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                // Período ya generado o mes no cerrado → lógica de negocio
                MessageBox.Show(ex.Message, "Operación No Permitida",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Error inesperado (BD, red, etc.)
                MessageBox.Show(
                    $"Error inesperado al generar las nóminas:\n{ex.Message}",
                    "Error del Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Eliminar nóminas de un periodo
        // ────────────────────────────────────────────────────────────────────

        private void BtnDeletePeriod_Click(object? sender, EventArgs e)
        {
            int year  = (int)numYear.Value;
            int month = cmbMonth.SelectedIndex + 1;

            try
            {
                var payments = MonthlyPayment.ReadByPeriod(year, month);
                if (payments.Count == 0)
                {
                    MessageBox.Show(
                        $"No hay nóminas registradas para el periodo {year}-{month:D2}.",
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"¿Está seguro de que desea ELIMINAR las {payments.Count} nóminas del periodo {year}-{month:D2}?\n\n" +
                    "Esta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // Borrado en una única sentencia: o se eliminan todas o ninguna
                int deleted = MonthlyPayment.DeleteByPeriod(year, month);

                MessageBox.Show(
                    $"{deleted} nóminas eliminadas del periodo {year}-{month:D2}.",
                    "Eliminación Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblSummary.Text = $"Periodo {year}-{month:D2}: nóminas eliminadas.";
                dgvPayments.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al eliminar nóminas:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Consultar nóminas existentes
        // ────────────────────────────────────────────────────────────────────

        private void LoadPayments()
        {
            try
            {
                int year  = (int)numYear.Value;
                int month = cmbMonth.SelectedIndex + 1;

                var payments = MonthlyPayment.ReadByPeriod(year, month);

                // Construir tabla con columnas legibles
                var table = new System.Data.DataTable();
                table.Columns.Add("ID",              typeof(int));
                table.Columns.Add("Empleado",        typeof(string));
                table.Columns.Add("Año",             typeof(int));
                table.Columns.Add("Mes",             typeof(int));
                table.Columns.Add("Bruto (€)",       typeof(decimal));
                table.Columns.Add("Neto (€)",        typeof(decimal));
                table.Columns.Add("Bonus Aplicado %",typeof(decimal));

                foreach (var p in payments)
                    table.Rows.Add(p.IdMonthlyPayment, p.Employee.Name,
                                   p.Year, p.Month,
                                   p.SalaryGross, p.SalaryNet, p.BonusApplied);

                dgvPayments.DataSource = table;

                // Formato monetario
                foreach (string col in new[] { "Bruto (€)", "Neto (€)" })
                    if (dgvPayments.Columns.Contains(col))
                    {
                        dgvPayments.Columns[col].DefaultCellStyle.Format    = "N2";
                        dgvPayments.Columns[col].DefaultCellStyle.Alignment =
                            DataGridViewContentAlignment.MiddleRight;
                    }

                if (payments.Count > 0)
                {
                    decimal totalGross = payments.Sum(p => p.SalaryGross);
                    decimal totalNet   = payments.Sum(p => p.SalaryNet);
                    UpdateSummary(year, month, payments.Count, totalGross, totalNet);
                }
                else
                {
                    lblSummary.Text = $"No hay nóminas registradas para el periodo {year}-{month:D2}.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar nóminas:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Utilidades
        // ────────────────────────────────────────────────────────────────────

        private void UpdateSummary(int year, int month, int count, decimal gross, decimal net)
        {
            lblSummary.Text =
                $"Periodo {year}-{month:D2}  |  {count} nóminas  |  " +
                $"Total Bruto: {gross:N2} €  |  Total Neto: {net:N2} €";
        }
    }
}
