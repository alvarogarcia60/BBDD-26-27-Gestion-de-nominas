using System;
using System.Drawing;
using System.Windows.Forms;
using GestionNominas.Domain.Entities;

namespace GestionNominas.Presentation.Forms
{
    public class FrmEmployees : Form
    {
        private DataGridView dgvEmployees = null!;
        private TextBox txtIdEmployee = null!;
        private TextBox txtName = null!;
        private ComboBox cmbCountry = null!;
        private TextBox txtIBAN = null!;
        private Button btnSave = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public FrmEmployees()
        {
            InitializeComponentManual();
            LoadCountries();
            LoadData();
        }

        private void InitializeComponentManual()
        {
            Text = "Gestión de Empleados (Employees)";
            Size = new Size(820, 560);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            var panelForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                Padding = new Padding(15)
            };

            var lblId = new Label { Text = "ID Empleado:", Location = new Point(20, 20), AutoSize = true };
            txtIdEmployee = new TextBox { Location = new Point(140, 17), Width = 80, ReadOnly = true, PlaceholderText = "Auto" };

            var lblName = new Label { Text = "Nombre Completo:", Location = new Point(20, 55), AutoSize = true };
            txtName = new TextBox { Location = new Point(140, 52), Width = 360, MaxLength = 120 };

            var lblCountry = new Label { Text = "País:", Location = new Point(20, 90), AutoSize = true };
            cmbCountry = new ComboBox { Location = new Point(140, 87), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblIBAN = new Label { Text = "Código IBAN:", Location = new Point(20, 125), AutoSize = true };
            txtIBAN = new TextBox { Location = new Point(140, 122), Width = 360, MaxLength = 34 };

            btnSave = new Button { Text = "Guardar", Location = new Point(540, 20), Width = 110, Height = 34, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button { Text = "Eliminar", Location = new Point(540, 62), Width = 110, Height = 34, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Limpiar", Location = new Point(540, 104), Width = 110, Height = 34 };
            btnClear.Click += (s, e) => ClearForm();

            panelForm.Controls.AddRange(new Control[] { lblId, txtIdEmployee, lblName, txtName, lblCountry, cmbCountry, lblIBAN, txtIBAN, btnSave, btnDelete, btnClear });

            dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            // Solo se pasa a modo edición al hacer clic en una fila, para no sobrescribir un registro al dar de alta otro.
            dgvEmployees.CellClick += (s, e) => { if (e.RowIndex >= 0) DgvEmployees_SelectionChanged(s, e); };
            dgvEmployees.DataBindingComplete += (s, e) => ClearForm();
            Shown += (s, e) => ClearForm();

            Controls.Add(dgvEmployees);
            Controls.Add(panelForm);

            UiTheme.Apply(this, "Empleados", "Datos personales, país e IBAN");
        }

        private void LoadCountries()
        {
            try
            {
                var countries = Country.ReadAll();
                cmbCountry.DataSource = countries;
                cmbCountry.DisplayMember = "Name";
                cmbCountry.ValueMember = "IdCountry";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar países: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                var employees = Employee.ReadAll();
                dgvEmployees.DataSource = null;
                dgvEmployees.DataSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar empleados: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvEmployees_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0 && dgvEmployees.SelectedRows[0].DataBoundItem is Employee emp)
            {
                btnSave.Text = "Actualizar";
                txtIdEmployee.Text = emp.IdEmployee.ToString();
                txtName.Text = emp.Name;
                txtIBAN.Text = emp.IBAN;

                if (emp.Country != null)
                {
                    cmbCountry.SelectedValue = emp.Country.IdCountry;
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbCountry.SelectedItem is not Country selectedCountry)
                {
                    MessageBox.Show("Seleccione un país para el empleado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = string.IsNullOrWhiteSpace(txtIdEmployee.Text) ? 0 : int.Parse(txtIdEmployee.Text);
                var employee = new Employee(id, txtName.Text, selectedCountry, txtIBAN.Text);

                bool success = id > 0 ? employee.Update() : employee.Insert();

                if (success)
                {
                    MessageBox.Show("Empleado guardado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar empleado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdEmployee.Text)) return;

            var confirm = MessageBox.Show("¿Desea eliminar el empleado seleccionado?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var emp = new Employee { IdEmployee = int.Parse(txtIdEmployee.Text) };
                    if (emp.Delete())
                    {
                        MessageBox.Show("Empleado eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadData();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Tiene puestos o nóminas asociados: no se puede eliminar
                    MessageBox.Show(ex.Message, "No se puede eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo eliminar el empleado:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            btnSave.Text = "Guardar nuevo";
            txtIdEmployee.Clear();
            txtName.Clear();
            txtIBAN.Clear();
            if (cmbCountry.Items.Count > 0) cmbCountry.SelectedIndex = 0;
            dgvEmployees.ClearSelection();
        }
    }
}
