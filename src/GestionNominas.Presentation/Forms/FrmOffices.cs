using System;
using System.Drawing;
using System.Windows.Forms;
using GestionNominas.Domain.Entities;

namespace GestionNominas.Presentation.Forms
{
    public class FrmOffices : Form
    {
        private DataGridView dgvOffices = null!;
        private TextBox txtIdOffice = null!;
        private ComboBox cmbCountry = null!;
        private TextBox txtCity = null!;
        private TextBox txtAddress = null!;
        private NumericUpDown numBonus = null!;
        private Button btnSave = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public FrmOffices()
        {
            InitializeComponentManual();
            LoadCountries();
            LoadData();
        }

        private void InitializeComponentManual()
        {
            Text = "Gestión de Oficinas (Offices)";
            Size = new Size(820, 560);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            var panelForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 175,
                Padding = new Padding(15)
            };

            var lblId = new Label { Text = "ID Oficina:", Location = new Point(20, 20), AutoSize = true };
            txtIdOffice = new TextBox { Location = new Point(140, 17), Width = 80, ReadOnly = true, PlaceholderText = "Auto" };

            var lblCountry = new Label { Text = "País:", Location = new Point(250, 20), AutoSize = true };
            cmbCountry = new ComboBox { Location = new Point(300, 17), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblCity = new Label { Text = "Ciudad:", Location = new Point(20, 55), AutoSize = true };
            txtCity = new TextBox { Location = new Point(140, 52), Width = 360, MaxLength = 60 };

            var lblAddress = new Label { Text = "Dirección:", Location = new Point(20, 90), AutoSize = true };
            txtAddress = new TextBox { Location = new Point(140, 87), Width = 360, MaxLength = 120 };

            var lblBonus = new Label { Text = "Bonus Trienio (%):", Location = new Point(20, 125), AutoSize = true };
            numBonus = new NumericUpDown { Location = new Point(140, 123), Width = 100, DecimalPlaces = 2, Minimum = 0, Maximum = 100, Value = 5.00m };

            btnSave = new Button { Text = "Guardar", Location = new Point(540, 20), Width = 110, Height = 34, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button { Text = "Eliminar", Location = new Point(540, 62), Width = 110, Height = 34, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Limpiar", Location = new Point(540, 104), Width = 110, Height = 34 };
            btnClear.Click += (s, e) => ClearForm();

            panelForm.Controls.AddRange(new Control[] { lblId, txtIdOffice, lblCountry, cmbCountry, lblCity, txtCity, lblAddress, txtAddress, lblBonus, numBonus, btnSave, btnDelete, btnClear });

            dgvOffices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvOffices.SelectionChanged += DgvOffices_SelectionChanged;

            Controls.Add(dgvOffices);
            Controls.Add(panelForm);
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
                MessageBox.Show($"Error al cargar lista de países: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                var offices = Office.ReadAll();
                dgvOffices.DataSource = null;
                dgvOffices.DataSource = offices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar oficinas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOffices_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvOffices.SelectedRows.Count > 0 && dgvOffices.SelectedRows[0].DataBoundItem is Office o)
            {
                txtIdOffice.Text = o.IdOffice.ToString();
                txtCity.Text = o.City;
                txtAddress.Text = o.Address;
                numBonus.Value = o.ThreeYearBonus;

                if (o.Country != null)
                {
                    cmbCountry.SelectedValue = o.Country.IdCountry;
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbCountry.SelectedItem is not Country selectedCountry)
                {
                    MessageBox.Show("Seleccione un país para la oficina.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = string.IsNullOrWhiteSpace(txtIdOffice.Text) ? 0 : int.Parse(txtIdOffice.Text);
                var office = new Office(id, selectedCountry, txtCity.Text, txtAddress.Text, numBonus.Value);

                bool success = id > 0 ? office.Update() : office.Insert();

                if (success)
                {
                    MessageBox.Show("Oficina guardada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar oficina: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdOffice.Text)) return;

            var confirm = MessageBox.Show($"¿Desea eliminar la oficina seleccionada?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var office = new Office { IdOffice = int.Parse(txtIdOffice.Text) };
                    if (office.Delete())
                    {
                        MessageBox.Show("Oficina eliminada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo eliminar la oficina (puede tener puestos asignados):\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            txtIdOffice.Clear();
            txtCity.Clear();
            txtAddress.Clear();
            numBonus.Value = 5.00m;
            if (cmbCountry.Items.Count > 0) cmbCountry.SelectedIndex = 0;
            dgvOffices.ClearSelection();
        }
    }
}
