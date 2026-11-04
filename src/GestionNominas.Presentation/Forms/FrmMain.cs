using System;
using System.Drawing;
using System.Windows.Forms;
using GestionNominas.Persistence;

namespace GestionNominas.Presentation.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
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

            bool isConnected = DBBroker.GetInstance().TestConnection(out string errorMessage);

            if (isConnected)
            {
                lblStatus.Text = $"✔ Conectado exitosamente a la base de datos MySQL ({DatabaseConfig.Database}) en {DatabaseConfig.Server}:{DatabaseConfig.Port}";
                lblStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                lblStatus.Text = "❌ Error de conexión con MySQL. Revise los parámetros de conexión.";
                lblStatus.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    $"No se pudo conectar a la base de datos MySQL:\n\n{errorMessage}\n\n" +
                    $"Servidor: {DatabaseConfig.Server}\n" +
                    $"Puerto: {DatabaseConfig.Port}\n" +
                    $"Base de datos: {DatabaseConfig.Database}\n" +
                    $"Usuario: {DatabaseConfig.UserId}\n\n" +
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
