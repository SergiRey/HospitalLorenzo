using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalLorenzo
{
    public sealed partial class CitasPage : Page
    {
        private static readonly string DataPathCitas =
            Path.Combine(AppContext.BaseDirectory, "citas.json");

        private List<Cita> _todasLasCitas = new();

        public CitasPage()
        {
            InitializeComponent();
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            var data = await CargarCitasAsync();
            _todasLasCitas = data.Citas;
            ListaCitas.ItemsSource = _todasLasCitas;
        }

        private async Task<CitasData> CargarCitasAsync()
        {
            try
            {
                if (!File.Exists(DataPathCitas)) return new CitasData();
                string json = await File.ReadAllTextAsync(DataPathCitas);
                return JsonSerializer.Deserialize<CitasData>(json)
                       ?? new CitasData();
            }
            catch { return new CitasData(); }
        }

        private async Task GuardarCitasAsync(CitasData data)
        {
            string json = JsonSerializer.Serialize(data,
                new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(DataPathCitas, json);
        }

        private void BtnNuevaCita_Click(object sender, RoutedEventArgs e)
        {
            PanelNuevaCita.Visibility = Visibility.Visible;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            PanelNuevaCita.Visibility = Visibility.Collapsed;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nuevaCita = new Cita
            {
                Id = _todasLasCitas.Count > 0
                     ? _todasLasCitas.Max(c => c.Id) + 1 : 1,
                PacienteNombre = txtPacienteNombre.Text,
                Motivo = txtMotivo.Text,
                Fecha = dpFechaCita.Date.HasValue
                        ? dpFechaCita.Date.Value.ToString("yyyy-MM-dd")
                        : DateTime.Today.ToString("yyyy-MM-dd"),
                Hora = tpHora.Time.Hours.ToString("00") + ":" + tpHora.Time.Minutes.ToString("00"),
                Estado = "Pendiente"
            };

            _todasLasCitas.Add(nuevaCita);
            await GuardarCitasAsync(new CitasData { Citas = _todasLasCitas });

            PanelNuevaCita.Visibility = Visibility.Collapsed;
            txtPacienteNombre.Text = string.Empty;
            txtMotivo.Text = string.Empty;
            tpHora.Time = TimeSpan.Zero;

            ListaCitas.ItemsSource = null;
            ListaCitas.ItemsSource = _todasLasCitas;
        }
    }
}