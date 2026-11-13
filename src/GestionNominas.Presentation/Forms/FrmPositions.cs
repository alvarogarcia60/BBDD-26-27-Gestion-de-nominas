using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestionNominas.Domain.Entities;

namespace GestionNominas.Presentation.Forms
{
    public class FrmPositions : Form
    {
        private DataGridView dgvPositions = null!;
        private TextBox txtIdPosition = null!;
        private ComboBox cmbEmployee = null!;
        private ComboBox cmbOffice = null!;
        private TextBox txtName = null!;
        private NumericUpDown numSalary = null!;
        private DateTimePicker dtpStart = null!;
        private CheckBox chkHasEnd = null!;
        private DateTimePicker dtpEnd = null!;
        private Button btnSave = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public FrmPositions()
        {
            InitializeComponentManual();
            LoadCombos();
            LoadData();
        }

        private void InitializeComponentManual()
        {
            Text = "Gestión de Puestos de Trabajo (Positions)";
            Size = new Size(950, 620);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            var panelForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 210,
                Padding = new Padding(15)
            };

            var lblId = new Label { Text = "ID Puesto:", Location = new Point(20, 20), AutoSize = true };
            txtIdPosition = new TextBox { Location = new Point(140, 17), Width = 80, ReadOnly = true, PlaceholderText = "Auto" };

            var lblEmp = new Label { Text = "Empleado:", Location = new Point(240, 20), AutoSize = true };
            cmbEmployee = new ComboBox { Location = new Point(310, 17), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblOffice = new Label { Text = "Oficina / Sede:", Location = new Point(20, 55), AutoSize = true };
            cmbOffice = new ComboBox { Location = new Point(140, 52), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblName = new Label { Text = "Cargo / Puesto:", Location = new Point(20, 90), AutoSize = true };
            txtName = new TextBox { Location = new Point(140, 87), Width = 280, MaxLength = 100 };

            var lblSalary = new Label { Text = "Salario Anual (€):", Location = new Point(440, 90), AutoSize = true };
            numSalary = new NumericUpDown { Location = new Point(540, 88), Width = 130, DecimalPlaces = 2, Minimum = 0, Maximum = 1000000, Value = 30000m };

            var lblStart = new Label { Text = "Fecha Inicio:", Location = new Point(20, 125), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(140, 122), Width = 140, Format = DateTimePickerFormat.Short };

            chkHasEnd = new CheckBox { Text = "Fecha Fin (Finalizado):", Location = new Point(310, 124), AutoSize = true };
            chkHasEnd.CheckedChanged += (s, e) => dtpEnd.Enabled = chkHasEnd.Checked;

            dtpEnd = new DateTimePicker { Location = new Point(470, 122), Width = 140, Format = DateTimePickerFormat.Short, Enabled = false };

            btnSave = new Button { Text = "Guardar", Location = new Point(720, 20), Width = 120, Height = 36, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button { Text = "Eliminar", Location = new Point(720, 68), Width = 120, Height = 36, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Limpiar", Location = new Point(720, 116), Width = 120, Height = 36 };
            btnClear.Click += (s, e) => ClearForm();

            panelForm.Controls.AddRange(new Control[] {
                lblId, txtIdPosition, lblEmp, cmbEmployee,
                lblOffice, cmbOffice,
                lblName, txtName, lblSalary, numSalary,
                lblStart, dtpStart, chkHasEnd, dtpEnd,
                btnSave, btnDelete, btnClear
            });

            dgvPositions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            // Solo se pasa a modo edición al hacer clic en una fila, para no sobrescribir un registro al dar de alta otro.
            dgvPositions.CellClick += (s, e) => { if (e.RowIndex >= 0) DgvPositions_SelectionChanged(s, e); };
            dgvPositions.DataBindingComplete += (s, e) => ClearForm();
            Shown += (s, e) => ClearForm();

            Controls.Add(dgvPositions);
            Controls.Add(panelForm);

            UiTheme.Apply(this, "Puestos de trabajo", "Historial de puestos, oficinas y salarios",
                new Dictionary<string, string> { { "Name", "Cargo" } });
        }

        private void LoadCombos()
        {
            try
            {
                var employees = Employee.ReadAll();
                cmbEmployee.DataSource = employees;
                cmbEmployee.DisplayMember = "Name";
                cmbEmployee.ValueMember = "IdEmployee";

                var offices = Office.ReadAll();
                cmbOffice.DataSource = offices;
                // "ToString" no es una propiedad: se formatea cada oficina con su descripción
                cmbOffice.FormattingEnabled = true;
                cmbOffice.Format += (s, e) => e.Value = e.ListItem?.ToString() ?? string.Empty;
                cmbOffice.ValueMember = "IdOffice";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar listas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                var positions = Position.ReadAll();
                dgvPositions.DataSource = null;
                dgvPositions.DataSource = positions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar puestos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPositions_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvPositions.SelectedRows.Count > 0 && dgvPositions.SelectedRows[0].DataBoundItem is Position pos)
            {
                btnSave.Text = "Actualizar";
                txtIdPosition.Text = pos.IdPosition.ToString();
                txtName.Text = pos.Name;
                numSalary.Value = pos.YearSalary;
                dtpStart.Value = pos.StartDate;

                if (pos.Employee != null)
                {
                    cmbEmployee.SelectedValue = pos.Employee.IdEmployee;
                }

                if (pos.Office != null)
                {
                    cmbOffice.SelectedValue = pos.Office.IdOffice;
                }

                if (pos.EndDate.HasValue)
                {
                    chkHasEnd.Checked = true;
                    dtpEnd.Value = pos.EndDate.Value;
                }
                else
                {
                    chkHasEnd.Checked = false;
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbEmployee.SelectedItem is not Employee emp)
                {
                    MessageBox.Show("Seleccione un empleado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbOffice.SelectedItem is not Office off)
                {
                    MessageBox.Show("Seleccione una oficina.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime? endDate = chkHasEnd.Checked ? dtpEnd.Value.Date : null;
                int id = string.IsNullOrWhiteSpace(txtIdPosition.Text) ? 0 : int.Parse(txtIdPosition.Text);

                var position = new Position(id, emp, off, txtName.Text, numSalary.Value, dtpStart.Value.Date, endDate);

                bool success = id > 0 ? position.Update() : position.Insert();

                if (success)
                {
                    MessageBox.Show("Puesto de trabajo guardado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar puesto: {ex.Message}", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPosition.Text)) return;

            var confirm = MessageBox.Show($"¿Desea eliminar la asignación de puesto seleccionada?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var pos = new Position { IdPosition = int.Parse(txtIdPosition.Text) };
                    if (pos.Delete())
                    {
                        MessageBox.Show("Puesto eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo eliminar el puesto:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            btnSave.Text = "Guardar nuevo";
            txtIdPosition.Clear();
            txtName.Clear();
            numSalary.Value = 30000m;
            dtpStart.Value = DateTime.Today;
            chkHasEnd.Checked = false;
            dtpEnd.Value = DateTime.Today;
            dgvPositions.ClearSelection();
        }
    }
}
