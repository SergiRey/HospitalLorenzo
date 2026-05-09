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
            CargarDatosIniciales();
        }

        private async void CargarDatosIniciales()
        {
            // Cargar Pacientes desde su JSON
            var pacientes = await LeerPacientesDesdeJson();
            cbPacientes.ItemsSource = pacientes;

            // Cargar Doctores desde su JSON
            var doctores = await LeerDoctoresDesdeJson();
            cbDoctores.ItemsSource = doctores;

            // Cargar Citas existentes
            _todasLasCitas = await LeerCitasDesdeJson();
            ListaCitas.ItemsSource = _todasLasCitas;
        }
        private async Task<List<Paciente>> LeerPacientesDesdeJson()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "pacientes.json");

            if (!File.Exists(path))
                return new List<Paciente>();

            string json = await File.ReadAllTextAsync(path);

            var data = JsonSerializer.Deserialize<PacientesData>(json);

            return data?.Pacientes ?? new List<Paciente>();
        }

        private async Task<List<Doctor>> LeerDoctoresDesdeJson()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "doctores.json");

            if (!File.Exists(path))
                return new List<Doctor>();

            string json = await File.ReadAllTextAsync(path);

            var data = JsonSerializer.Deserialize<DoctoresData>(json);

            return data?.Doctores ?? new List<Doctor>();
        }

        private async Task<List<Cita>> LeerCitasDesdeJson()
        {
            // Reutilizamos tu lógica de CargarCitasAsync pero devolviendo la lista
            var data = await CargarCitasAsync();
            return data.Citas ?? new List<Cita>();
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
            // Validación con el diálogo correcto para tu tipo de proyecto
            if (cbPacientes.SelectedValue == null || cbDoctores.SelectedValue == null)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Selecciona paciente y doctor");
                await dialog.ShowAsync();
                return;
            }

            var nuevaCita = new Cita
            {
                Id = _todasLasCitas.Count > 0 ? _todasLasCitas.Max(c => c.Id) + 1 : 1,

                // Conversión de String a Int para evitar el error CS0029
                PacienteId = Convert.ToInt32(cbPacientes.SelectedValue),
                DoctorId = Convert.ToInt32(cbDoctores.SelectedValue),

                Motivo = txtMotivo.Text,
                Fecha = dpFechaCita.Date.HasValue
                        ? dpFechaCita.Date.Value.ToString("yyyy-MM-dd")
                        : DateTime.Today.ToString("yyyy-MM-dd"),
                Hora = tpHora.Time.Hours.ToString("00") + ":" + tpHora.Time.Minutes.ToString("00"),
                Estado = "Pendiente"
            };

            // 1. Crea esta clase temporal o permanente
           

            _todasLasCitas.Add(nuevaCita);
            await GuardarCitasAsync(new CitasData { Citas = _todasLasCitas });

            // Limpieza de campos
            PanelNuevaCita.Visibility = Visibility.Collapsed;
            cbPacientes.SelectedIndex = -1;
            cbDoctores.SelectedIndex = -1;
            txtMotivo.Text = string.Empty;

            ListaCitas.ItemsSource = null;
            ListaCitas.ItemsSource = _todasLasCitas;
        }
    }
}