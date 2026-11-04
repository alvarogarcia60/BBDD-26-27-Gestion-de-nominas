namespace GestionNominas.Presentation.Forms
{
    partial class FrmMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuMantenimiento = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPaises = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOficinas = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEmpleados = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPuestos = new System.Windows.Forms.ToolStripMenuItem();
            this.menuNominas = new System.Windows.Forms.ToolStripMenuItem();
            this.menuGenerarNominas = new System.Windows.Forms.ToolStripMenuItem();
            this.menuInformes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVerInformes = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblSub = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelDashboard = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPaises = new System.Windows.Forms.Button();
            this.btnOficinas = new System.Windows.Forms.Button();
            this.btnEmpleados = new System.Windows.Forms.Button();
            this.btnPuestos = new System.Windows.Forms.Button();
            this.btnNominas = new System.Windows.Forms.Button();
            this.btnInformes = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelDashboard.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuMantenimiento,
            this.menuNominas,
            this.menuInformes});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(900, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuMantenimiento
            // 
            this.menuMantenimiento.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPaises,
            this.menuOficinas,
            this.menuEmpleados,
            this.menuPuestos});
            this.menuMantenimiento.Name = "menuMantenimiento";
            this.menuMantenimiento.Size = new System.Drawing.Size(101, 20);
            this.menuMantenimiento.Text = "Mantenimiento";
            // 
            // menuPaises
            // 
            this.menuPaises.Name = "menuPaises";
            this.menuPaises.Size = new System.Drawing.Size(132, 22);
            this.menuPaises.Text = "Países";
            this.menuPaises.Click += new System.EventHandler(this.btnPaises_Click);
            // 
            // menuOficinas
            // 
            this.menuOficinas.Name = "menuOficinas";
            this.menuOficinas.Size = new System.Drawing.Size(132, 22);
            this.menuOficinas.Text = "Oficinas";
            this.menuOficinas.Click += new System.EventHandler(this.btnOficinas_Click);
            // 
            // menuEmpleados
            // 
            this.menuEmpleados.Name = "menuEmpleados";
            this.menuEmpleados.Size = new System.Drawing.Size(132, 22);
            this.menuEmpleados.Text = "Empleados";
            this.menuEmpleados.Click += new System.EventHandler(this.btnEmpleados_Click);
            // 
            // menuPuestos
            // 
            this.menuPuestos.Name = "menuPuestos";
            this.menuPuestos.Size = new System.Drawing.Size(132, 22);
            this.menuPuestos.Text = "Puestos";
            this.menuPuestos.Click += new System.EventHandler(this.btnPuestos_Click);
            // 
            // menuNominas
            // 
            this.menuNominas.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuGenerarNominas});
            this.menuNominas.Name = "menuNominas";
            this.menuNominas.Size = new System.Drawing.Size(67, 20);
            this.menuNominas.Text = "Nóminas";
            // 
            // menuGenerarNominas
            // 
            this.menuGenerarNominas.Name = "menuGenerarNominas";
            this.menuGenerarNominas.Size = new System.Drawing.Size(164, 22);
            this.menuGenerarNominas.Text = "Generar Nóminas";
            this.menuGenerarNominas.Click += new System.EventHandler(this.btnNominas_Click);
            // 
            // menuInformes
            // 
            this.menuInformes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuVerInformes});
            this.menuInformes.Name = "menuInformes";
            this.menuInformes.Size = new System.Drawing.Size(66, 20);
            this.menuInformes.Text = "Informes";
            // 
            // menuVerInformes
            // 
            this.menuVerInformes.Name = "menuVerInformes";
            this.menuVerInformes.Size = new System.Drawing.Size(154, 22);
            this.menuVerInformes.Text = "Ver Informes...";
            this.menuVerInformes.Click += new System.EventHandler(this.btnInformes_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 538);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(900, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(161, 17);
            this.lblStatus.Text = "Conectando a la Base de Datos...";
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(54)))), ((int)(((byte)(93)))));
            this.panelHeader.Controls.Add(this.lblSub);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 24);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(900, 80);
            this.panelHeader.TabIndex = 2;
            // 
            // lblSub
            // 
            this.lblSub.AutoSize = true;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSub.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.lblSub.Location = new System.Drawing.Point(24, 46);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(370, 17);
            this.lblSub.TabIndex = 1;
            this.lblSub.Text = "Arquitectura 3 Capas (Presentación, Dominio POO, Persistencia)";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(22, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(437, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Sistema de Gestión de Pagos y Nóminas";
            // 
            // panelDashboard
            // 
            this.panelDashboard.AutoScroll = true;
            this.panelDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.panelDashboard.Controls.Add(this.btnPaises);
            this.panelDashboard.Controls.Add(this.btnOficinas);
            this.panelDashboard.Controls.Add(this.btnEmpleados);
            this.panelDashboard.Controls.Add(this.btnPuestos);
            this.panelDashboard.Controls.Add(this.btnNominas);
            this.panelDashboard.Controls.Add(this.btnInformes);
            this.panelDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDashboard.Location = new System.Drawing.Point(0, 104);
            this.panelDashboard.Name = "panelDashboard";
            this.panelDashboard.Padding = new System.Windows.Forms.Padding(30);
            this.panelDashboard.Size = new System.Drawing.Size(900, 434);
            this.panelDashboard.TabIndex = 3;
            // 
            // btnPaises
            // 
            this.btnPaises.BackColor = System.Drawing.Color.White;
            this.btnPaises.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPaises.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPaises.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnPaises.Location = new System.Drawing.Point(33, 33);
            this.btnPaises.Margin = new System.Windows.Forms.Padding(3, 3, 20, 20);
            this.btnPaises.Name = "btnPaises";
            this.btnPaises.Size = new System.Drawing.Size(250, 110);
            this.btnPaises.TabIndex = 0;
            this.btnPaises.Text = "🌍  Gestión de Países\r\n(Codificación y Tasas)";
            this.btnPaises.UseVisualStyleBackColor = false;
            this.btnPaises.Click += new System.EventHandler(this.btnPaises_Click);
            // 
            // btnOficinas
            // 
            this.btnOficinas.BackColor = System.Drawing.Color.White;
            this.btnOficinas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOficinas.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOficinas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnOficinas.Location = new System.Drawing.Point(306, 33);
            this.btnOficinas.Margin = new System.Windows.Forms.Padding(3, 3, 20, 20);
            this.btnOficinas.Name = "btnOficinas";
            this.btnOficinas.Size = new System.Drawing.Size(250, 110);
            this.btnOficinas.TabIndex = 1;
            this.btnOficinas.Text = "🏢  Gestión de Oficinas\r\n(Sedes y Trienios Base)";
            this.btnOficinas.UseVisualStyleBackColor = false;
            this.btnOficinas.Click += new System.EventHandler(this.btnOficinas_Click);
            // 
            // btnEmpleados
            // 
            this.btnEmpleados.BackColor = System.Drawing.Color.White;
            this.btnEmpleados.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEmpleados.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnEmpleados.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnEmpleados.Location = new System.Drawing.Point(579, 33);
            this.btnEmpleados.Margin = new System.Windows.Forms.Padding(3, 3, 20, 20);
            this.btnEmpleados.Name = "btnEmpleados";
            this.btnEmpleados.Size = new System.Drawing.Size(250, 110);
            this.btnEmpleados.TabIndex = 2;
            this.btnEmpleados.Text = "👥  Gestión de Empleados\r\n(Datos, Países e IBAN)";
            this.btnEmpleados.UseVisualStyleBackColor = false;
            this.btnEmpleados.Click += new System.EventHandler(this.btnEmpleados_Click);
            // 
            // btnPuestos
            // 
            this.btnPuestos.BackColor = System.Drawing.Color.White;
            this.btnPuestos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPuestos.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPuestos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.btnPuestos.Location = new System.Drawing.Point(33, 166);
            this.btnPuestos.Margin = new System.Windows.Forms.Padding(3, 3, 20, 20);
            this.btnPuestos.Name = "btnPuestos";
            this.btnPuestos.Size = new System.Drawing.Size(250, 110);
            this.btnPuestos.TabIndex = 3;
            this.btnPuestos.Text = "💼  Puestos de Trabajo\r\n(Historial y Salarios)";
            this.btnPuestos.UseVisualStyleBackColor = false;
            this.btnPuestos.Click += new System.EventHandler(this.btnPuestos_Click);
            // 
            // btnNominas
            // 
            this.btnNominas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnNominas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNominas.Font = new System.Drawing.Font("Segoe UI Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnNominas.ForeColor = System.Drawing.Color.White;
            this.btnNominas.Location = new System.Drawing.Point(306, 166);
            this.btnNominas.Margin = new System.Windows.Forms.Padding(3, 3, 20, 20);
            this.btnNominas.Name = "btnNominas";
            this.btnNominas.Size = new System.Drawing.Size(250, 110);
            this.btnNominas.TabIndex = 4;
            this.btnNominas.Text = "💵  Generación de Nóminas\r\n(Cálculo Mensual Automático)";
            this.btnNominas.UseVisualStyleBackColor = false;
            this.btnNominas.Click += new System.EventHandler(this.btnNominas_Click);
            // 
            // btnInformes
            // 
            this.btnInformes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnInformes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInformes.Font = new System.Drawing.Font("Segoe UI Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnInformes.ForeColor = System.Drawing.Color.White;
            this.btnInformes.Location = new System.Drawing.Point(579, 166);
            this.btnInformes.Margin = new System.Windows.Forms.Padding(3, 3, 20, 20);
            this.btnInformes.Name = "btnInformes";
            this.btnInformes.Size = new System.Drawing.Size(250, 110);
            this.btnInformes.TabIndex = 5;
            this.btnInformes.Text = "📊  Informes y Consultas\r\n(Estadísticas y Listados)";
            this.btnInformes.UseVisualStyleBackColor = false;
            this.btnInformes.Click += new System.EventHandler(this.btnInformes_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.panelDashboard);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gestión de Nóminas - Aplicación de Escritorio";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelDashboard.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuMantenimiento;
        private System.Windows.Forms.ToolStripMenuItem menuPaises;
        private System.Windows.Forms.ToolStripMenuItem menuOficinas;
        private System.Windows.Forms.ToolStripMenuItem menuEmpleados;
        private System.Windows.Forms.ToolStripMenuItem menuPuestos;
        private System.Windows.Forms.ToolStripMenuItem menuNominas;
        private System.Windows.Forms.ToolStripMenuItem menuGenerarNominas;
        private System.Windows.Forms.ToolStripMenuItem menuInformes;
        private System.Windows.Forms.ToolStripMenuItem menuVerInformes;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.FlowLayoutPanel panelDashboard;
        private System.Windows.Forms.Button btnPaises;
        private System.Windows.Forms.Button btnOficinas;
        private System.Windows.Forms.Button btnEmpleados;
        private System.Windows.Forms.Button btnPuestos;
        private System.Windows.Forms.Button btnNominas;
        private System.Windows.Forms.Button btnInformes;
    }
}
