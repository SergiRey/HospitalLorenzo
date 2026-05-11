using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace HospitalLorenzo
{
    public sealed partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            this.InitializeComponent();
            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            IniciarReloj();
        }

        private bool _esFullscreen = true;

        private void BtnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (_esFullscreen)
            {
                AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                _esFullscreen = false;
            }
            else
            {
                AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                _esFullscreen = true;
            }
        }

        private void IniciarReloj()
        {
            ActualizarFechaHora();

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => ActualizarFechaHora();
            timer.Start();
        }

        private void ActualizarFechaHora()
        {
            var ahora = DateTime.Now;
            FechaText.Text = ahora.ToString("dd/MM/yyyy");
            HoraText.Text = ahora.ToString("HH:mm");
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPacientes_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(PacientesPage));

        }

        private void BtnDoctores_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(DoctoresPage));
        }

        private void BtnCitas_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(CitasPage));
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ReportesPage));
        }
    }
}