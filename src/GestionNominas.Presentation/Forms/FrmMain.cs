using System;
using System.Drawing;
using System.Windows.Forms;
using GestionNominas.Domain.Services;

namespace GestionNominas.Presentation.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            UiTheme.ApplyMain(panelHeader, panelDashboard, statusStrip1, menuStrip1);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // Requisito del PDF: "Al iniciar la aplicación, el sistema se conectará automáticamente a la base de datos (BD)"
            CheckDatabaseConnection();
        }

        private void CheckDatabaseConnection()
        {
            lblStatus.Text = "Conectando al servidor MySQL...";
            lblStatus.ForeColor = Color.Black;

            bool isConnected = DatabaseService.TestConnection(out string errorMessage);

            if (isConnected)
            {
                lblStatus.Text = $"✔ Conectado exitosamente a la base de datos MySQL ({DatabaseService.ShortDescription})";
                lblStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblStatus.Text = "❌ Error de conexión con MySQL. Revise los parámetros de conexión.";
                lblStatus.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    $"No se pudo conectar a la base de datos MySQL:\n\n{errorMessage}\n\n" +
                    $"{DatabaseService.ConnectionDescription}\n\n" +
                    "Asegúrese de que el servicio MySQL esté iniciado y de haber ejecutado el script SQL de creación de la base de datos.",
                    "Aviso de Conexión a Base de Datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnPaises_Click(object sender, EventArgs e)
        {
            using var form = new FrmCountries();
            form.ShowDialog(this);
        }

        private void btnOficinas_Click(object sender, EventArgs e)
        {
            using var form = new FrmOffices();
            form.ShowDialog(this);
        }

        private void btnEmpleados_Click(object sender, EventArgs e)
        {
            using var form = new FrmEmployees();
            form.ShowDialog(this);
        }

        private void btnPuestos_Click(object sender, EventArgs e)
        {
            using var form = new FrmPositions();
            form.ShowDialog(this);
        }

        private void btnNominas_Click(object sender, EventArgs e)
        {
            using var form = new FrmMonthlyPayments();
            form.ShowDialog(this);
        }

        private void btnInformes_Click(object sender, EventArgs e)
        {
            using var form = new FrmReports();
            form.ShowDialog(this);
        }
    }
}
