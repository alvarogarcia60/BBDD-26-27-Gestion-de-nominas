using System;
using System.Drawing;
using System.Windows.Forms;
using GestionNominas.Domain.Entities;
using GestionNominas.Domain.Services;

namespace GestionNominas.Presentation.Forms
{
    public class FrmCountries : Form
    {
        private DataGridView dgvCountries = null!;
        private TextBox txtIdCountry = null!;
        private TextBox txtName = null!;
        private NumericUpDown numTaxBase = null!;
        private Button btnAutoCode = null!;
        private Button btnSave = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public FrmCountries()
        {
            InitializeComponentManual();
            LoadData();
        }

        private void InitializeComponentManual()
        {
            Text = "Gestión de Países (Countries)";
            Size = new Size(750, 520);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            var panelForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(15)
            };

            var lblId = new Label { Text = "Código País (3 letras):", Location = new Point(20, 20), AutoSize = true };
            txtIdCountry = new TextBox { Location = new Point(160, 17), Width = 80, MaxLength = 3 };

            btnAutoCode = new Button { Text = "Sugerir Código", Location = new Point(250, 16), Width = 110, Height = 25 };
            btnAutoCode.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtName.Text))
                {
                    txtIdCountry.Text = CountryCodeGenerator.GenerateCode(txtName.Text);
                }
                else
                {
                    MessageBox.Show("Introduzca primero el nombre del país para sugerir el código.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            var lblName = new Label { Text = "Nombre del País:", Location = new Point(20, 55), AutoSize = true };
            txtName = new TextBox { Location = new Point(160, 52), Width = 280, MaxLength = 120 };

            var lblTax = new Label { Text = "Base Impositiva (%):", Location = new Point(20, 90), AutoSize = true };
            numTaxBase = new NumericUpDown { Location = new Point(160, 88), Width = 100, DecimalPlaces = 2, Minimum = 0, Maximum = 100, Value = 15.00m };

            btnSave = new Button { Text = "Guardar", Location = new Point(480, 20), Width = 100, Height = 32, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button { Text = "Eliminar", Location = new Point(480, 58), Width = 100, Height = 32, BackColor = Color.IndianRed, ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Limpiar", Location = new Point(480, 96), Width = 100, Height = 32 };
            btnClear.Click += (s, e) => ClearForm();

            panelForm.Controls.AddRange(new Control[] { lblId, txtIdCountry, btnAutoCode, lblName, txtName, lblTax, numTaxBase, btnSave, btnDelete, btnClear });

            dgvCountries = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvCountries.SelectionChanged += DgvCountries_SelectionChanged;

            Controls.Add(dgvCountries);
            Controls.Add(panelForm);
        }

        private void LoadData()
        {
            try
            {
                var countries = Country.ReadAll();
                dgvCountries.DataSource = null;
                dgvCountries.DataSource = countries;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar países: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCountries_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvCountries.SelectedRows.Count > 0 && dgvCountries.SelectedRows[0].DataBoundItem is Country c)
            {
                txtIdCountry.Text = c.IdCountry;
                txtIdCountry.ReadOnly = true;
                txtName.Text = c.Name;
                numTaxBase.Value = c.TaxBase;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                string id = txtIdCountry.Text.Trim().ToUpper();
                string name = txtName.Text.Trim();
                decimal tax = numTaxBase.Value;

                var country = new Country(id, name, tax);
                bool exists = Country.Exists(id);

                bool success;
                if (exists && txtIdCountry.ReadOnly)
                {
                    success = country.Update();
                }
                else
                {
                    success = country.Insert();
                }

                if (success)
                {
                    MessageBox.Show("País guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar país: {ex.Message}", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdCountry.Text)) return;

            var confirm = MessageBox.Show($"¿Desea eliminar el país {txtIdCountry.Text}?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var country = new Country { IdCountry = txtIdCountry.Text.Trim() };
                    if (country.Delete())
                    {
                        MessageBox.Show("País eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo eliminar el país (posiblemente tenga oficinas o empleados asociados):\n{ex.Message}", "Restricción de Integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            txtIdCountry.Clear();
            txtIdCountry.ReadOnly = false;
            txtName.Clear();
            numTaxBase.Value = 15.00m;
            dgvCountries.ClearSelection();
        }
    }
}
