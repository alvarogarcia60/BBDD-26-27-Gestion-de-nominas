using System;
using System.Threading;
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
            // Red de seguridad global: cualquier excepción no capturada en un formulario
            // se muestra al usuario en lugar de cerrar la aplicación de forma inesperada.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Se ha producido un error inesperado:\n\n{e.Exception.Message}\n\n" +
                "La operación no se ha completado, pero puede seguir usando la aplicación.",
                "Error inesperado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = e.ExceptionObject is Exception ex ? ex.Message : "Error desconocido.";
            MessageBox.Show(
                $"Se ha producido un error grave y la aplicación debe cerrarse:\n\n{message}",
                "Error grave",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
