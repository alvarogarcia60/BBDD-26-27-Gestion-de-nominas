using System;
using System.Windows.Forms;
using GestionNominas.Presentation.Forms;

namespace GestionNominas.Presentation
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación de escritorio.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }
    }
}
