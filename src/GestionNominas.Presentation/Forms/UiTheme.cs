using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GestionNominas.Presentation.Forms
{
    /// <summary>
    /// Estilo visual común de la capa de presentación. Solo afecta a la apariencia:
    /// no contiene lógica de negocio ni modifica el comportamiento de los formularios.
    /// </summary>
    internal static class UiTheme
    {
        private static readonly Color HeaderStart   = Color.FromArgb(15, 42, 74);
        private static readonly Color HeaderEnd     = Color.FromArgb(37, 99, 235);
        private static readonly Color Background    = Color.FromArgb(241, 245, 249);
        private static readonly Color Card          = Color.White;
        private static readonly Color Text          = Color.FromArgb(30, 41, 59);
        private static readonly Color Border        = Color.FromArgb(203, 213, 225);
        private static readonly Color GridLine      = Color.FromArgb(226, 232, 240);
        private static readonly Color AlternateRow  = Color.FromArgb(248, 250, 252);
        private static readonly Color Selection     = Color.FromArgb(219, 234, 254);
        private static readonly Color SelectionText = Color.FromArgb(30, 58, 138);
        private static readonly Color Primary       = Color.FromArgb(37, 99, 235);
        private static readonly Color Danger        = Color.FromArgb(220, 38, 38);

        private const int HeaderHeight = 68;
        private const string HeaderTag = "ui-theme-header";

        /// <summary>Títulos legibles para las columnas generadas desde las entidades de dominio.</summary>
        private static readonly Dictionary<string, string> ColumnTitles = new()
        {
            { "IdCountry", "Código" },
            { "Name", "Nombre" },
            { "TaxBase", "Impuesto (%)" },
            { "IdOffice", "ID" },
            { "Country", "País" },
            { "City", "Ciudad" },
            { "Address", "Dirección" },
            { "ThreeYearBonus", "Bonus trienio (%)" },
            { "IdEmployee", "ID" },
            { "IdPosition", "ID" },
            { "Employee", "Empleado" },
            { "Office", "Oficina" },
            { "YearSalary", "Salario anual (€)" },
            { "StartDate", "Inicio" },
            { "EndDate", "Fin" }
        };

        /// <summary>
        /// Aplica el tema a un formulario construido por código y lo adapta a la escala de la pantalla.
        /// Debe llamarse al final de la construcción de la interfaz, antes de cargar los datos.
        /// </summary>
        public static void Apply(Form form, string title, string subtitle,
                                 IReadOnlyDictionary<string, string>? columnOverrides = null)
        {
            float scale = form.DeviceDpi / 96f;

            form.BackColor = Background;
            form.Height += HeaderHeight;
            form.Controls.Add(BuildHeader(title, subtitle));

            StyleChildren(form, scale, columnOverrides);

            // Los formularios colocan sus controles en coordenadas fijas: se escalan junto con la
            // fuente para que las etiquetas no se monten sobre los campos con escalado de Windows.
            if (scale > 1.01f)
            {
                Size minimum = form.MinimumSize;
                form.Scale(new SizeF(scale, scale));
                if (!minimum.IsEmpty)
                    form.MinimumSize = new Size((int)(minimum.Width * scale), (int)(minimum.Height * scale));
            }

            FitToScreen(form);
        }

        /// <summary>Aplica el tema a la ventana principal, diseñada con el diseñador de Visual Studio.</summary>
        public static void ApplyMain(Panel header, Panel dashboard, StatusStrip status, MenuStrip menu)
        {
            foreach (Control label in header.Controls)
                label.BackColor = Color.Transparent;
            PaintGradient(header);

            dashboard.BackColor = Background;
            status.BackColor = Card;
            menu.BackColor = Card;

            foreach (Control control in dashboard.Controls)
            {
                if (control is not Button card) continue;

                card.Cursor = Cursors.Hand;
                bool isPlainCard = card.BackColor.ToArgb() == Color.White.ToArgb();
                card.FlatAppearance.BorderSize = isPlainCard ? 1 : 0;
                card.FlatAppearance.BorderColor = Border;
                card.FlatAppearance.MouseOverBackColor = isPlainCard ? Selection : Blend(card.BackColor, Color.White, 0.15f);
                card.FlatAppearance.MouseDownBackColor = Blend(card.BackColor, Color.Black, 0.10f);
            }
        }

        private static Panel BuildHeader(string title, string subtitle)
        {
            var header = new Panel { Dock = DockStyle.Top, Height = HeaderHeight, Tag = HeaderTag };

            header.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(20, 9),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 15F)
            });
            header.Controls.Add(new Label
            {
                Text = subtitle,
                AutoSize = true,
                Location = new Point(22, 42),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(191, 219, 254),
                Font = new Font("Segoe UI", 9F)
            });

            PaintGradient(header);
            return header;
        }

        private static void PaintGradient(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                Rectangle area = panel.ClientRectangle;
                if (area.Width <= 0 || area.Height <= 0) return; // ventana minimizada

                using var brush = new LinearGradientBrush(area, HeaderStart, HeaderEnd, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, area);
            };
            panel.Resize += (s, e) => panel.Invalidate();
        }

        private static void StyleChildren(Control parent, float scale, IReadOnlyDictionary<string, string>? columnOverrides)
        {
            foreach (Control control in parent.Controls)
            {
                if (HeaderTag.Equals(control.Tag)) continue;

                switch (control)
                {
                    case DataGridView grid: StyleGrid(grid, scale, columnOverrides); break;
                    case Button button: StyleButton(button); break;
                    case TabControl tabs: StyleTabs(tabs, scale); break;
                    case TabPage page:
                        page.UseVisualStyleBackColor = false;
                        page.BackColor = Background;
                        break;
                    case Panel { Dock: DockStyle.Top } panel: panel.BackColor = Card; break;
                    case TextBox textBox:
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        if (textBox.ReadOnly) textBox.BackColor = Background;
                        break;
                    case Label label when label.ForeColor == Control.DefaultForeColor:
                        label.ForeColor = Text;
                        break;
                }

                StyleChildren(control, scale, columnOverrides);
            }
        }

        private static void StyleButton(Button button)
        {
            int color = button.BackColor.ToArgb();
            // Un botón sin color propio hereda el fondo blanco de su panel
            bool isSecondary = button.UseVisualStyleBackColor || color == Card.ToArgb();

            if (color == Color.SteelBlue.ToArgb()) button.BackColor = Primary;
            else if (color == Color.IndianRed.ToArgb()) button.BackColor = Danger;
            else if (isSecondary) { button.BackColor = Card; button.ForeColor = Text; }

            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI Semibold", button.Font.Size);
            button.FlatAppearance.BorderSize = isSecondary ? 1 : 0;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = isSecondary ? Selection : Blend(button.BackColor, Color.White, 0.15f);
            button.FlatAppearance.MouseDownBackColor = Blend(button.BackColor, Color.Black, 0.10f);
        }

        private static void StyleGrid(DataGridView grid, float scale, IReadOnlyDictionary<string, string>? columnOverrides)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Background;
            grid.GridColor = GridLine;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = (int)(36 * scale);
            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderStart;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderStart;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            grid.RowTemplate.Height = (int)(30 * scale);
            grid.DefaultCellStyle.BackColor = Card;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Selection;
            grid.DefaultCellStyle.SelectionForeColor = SelectionText;
            grid.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = AlternateRow;

            grid.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (columnOverrides != null && columnOverrides.TryGetValue(column.Name, out string? custom))
                        column.HeaderText = custom;
                    else if (ColumnTitles.TryGetValue(column.Name, out string? title))
                        column.HeaderText = title;
                }
            };
        }

        private static void StyleTabs(TabControl tabs, float scale)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size((int)(200 * scale), (int)(34 * scale));

            tabs.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= tabs.TabPages.Count) return;

                bool selected = e.Index == tabs.SelectedIndex;
                using (var brush = new SolidBrush(selected ? HeaderStart : GridLine))
                    e.Graphics.FillRectangle(brush, e.Bounds);

                TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, e.Bounds,
                    selected ? Color.White : Text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            };
        }

        private static void FitToScreen(Form form)
        {
            Rectangle screen = Screen.FromControl(form).WorkingArea;
            form.Size = new Size(Math.Min(form.Width, screen.Width), Math.Min(form.Height, screen.Height));
        }

        private static Color Blend(Color color, Color target, float amount) => Color.FromArgb(
            (int)(color.R + (target.R - color.R) * amount),
            (int)(color.G + (target.G - color.G) * amount),
            (int)(color.B + (target.B - color.B) * amount));
    }
}
