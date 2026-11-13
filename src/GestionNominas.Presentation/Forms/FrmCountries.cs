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

        /// <summary>Código del país seleccionado para edición (null = alta de un país nuevo).</summary>
        private string? _editingId;

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
            // El código no se escribe a mano: se genera a partir del nombre
            txtIdCountry = new TextBox { Location = new Point(160, 17), Width = 80, MaxLength = 3, ReadOnly = true, PlaceholderText = "Auto" };

            btnAutoCode = new Button { Text = "Ver Código", Location = new Point(250, 16), Width = 110, Height = 25 };
            btnAutoCode.Click += (s, e) =>
            {
                if (_editingId != null)
                {
                    MessageBox.Show("El código de un país existente no se puede modificar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Introduzca primero el nombre del país para ver su código.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    txtIdCountry.Text = CountryCodeGenerator.GenerateCode(txtName.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo calcular el código: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var lblName = new Label { Text = "Nombre del País:", Location = new Point(20, 55), AutoSize = true };
            txtName = new TextBox { Location = new Point(160, 52), Width = 280, MaxLength = 120 };

            var lblTax = new Label { Text = "Base Impositiva (%):", Location = new Point(20, 90), AutoSize = true };
            numTaxBase = new NumericUpDown { Location = new Point(160, 88), Width = 100, DecimalPlaces = 2, Minimum = 0, Maximum = 99.99m, Value = 15.00m };

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
            // Solo se pasa a modo edición al hacer clic en una fila, para no sobrescribir un registro al dar de alta otro.
            dgvCountries.CellClick += (s, e) => { if (e.RowIndex >= 0) DgvCountries_SelectionChanged(s, e); };
            dgvCountries.DataBindingComplete += (s, e) => ClearForm();
            Shown += (s, e) => ClearForm();

            Controls.Add(dgvCountries);
            Controls.Add(panelForm);

            UiTheme.Apply(this, "Países", "Códigos de país e impuestos aplicables");
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
                btnSave.Text = "Actualizar";
                _editingId = c.IdCountry;
                txtIdCountry.Text = c.IdCountry;
                txtName.Text = c.Name;
                numTaxBase.Value = c.TaxBase;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                string name = txtName.Text.Trim();
                decimal tax = numTaxBase.Value;

                bool success;
                if (_editingId != null)
                {
                    // Edición: se conserva el código existente (clave primaria)
                    success = new Country(_editingId, name, tax).Update();
                }
                else
                {
                    // Alta: código vacío para que el dominio lo genere
                    var country = new Country(string.Empty, name, tax);
                    success = country.Insert();
                    if (success)
                    {
                        MessageBox.Show($"Se ha asignado el código {country.IdCountry} al país {country.Name}.", "Código generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
            if (_editingId == null)
            {
                MessageBox.Show("Seleccione en la lista el país que desea eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"¿Desea eliminar el país {_editingId}?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var country = new Country { IdCountry = _editingId };
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
            btnSave.Text = "Guardar nuevo";
            _editingId = null;
            txtIdCountry.Clear();
            txtName.Clear();
            numTaxBase.Value = 15.00m;
            dgvCountries.ClearSelection();
        }
    }
}
