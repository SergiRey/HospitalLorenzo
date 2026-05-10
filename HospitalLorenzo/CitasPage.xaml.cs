using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HospitalLorenzo
{
    public sealed partial class CitasPage : Page
    {
        private static readonly string DataPathCitas = Rutas.Citas;

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
            string path = Rutas.Pacientes;

            if (!File.Exists(path))
                return new List<Paciente>();

            string json = await File.ReadAllTextAsync(path);

            var data = JsonSerializer.Deserialize<PacientesData>(json);

            return data?.Pacientes ?? new List<Paciente>();
        }

        private async Task<List<Doctor>> LeerDoctoresDesdeJson()
        {
            string path = Rutas.Doctores;

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
            Rutas.AsegurarDirectorio(Rutas.Citas);
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
        private async void BtnPdfCita_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                int citaId = Convert.ToInt32(btn?.Tag);
                var cita = _todasLasCitas.FirstOrDefault(c => c.Id == citaId);
                if (cita == null) return;

                // Leer paciente y doctor del JSON
                var pacientes = await LeerPacientesDesdeJson();
                var doctores = await LeerDoctoresDesdeJson();

                var paciente = pacientes.FirstOrDefault(p => p.Id == cita.PacienteId);
                var doctor = doctores.FirstOrDefault(d => d.Id == cita.DoctorId);

                string nombrePaciente = paciente?.Nombre ?? "Sin nombre";
                string nombreDoctor = doctor?.Nombre ?? "Sin nombre";
                string especialidad = doctor?.Especialidad ?? cita.Especialidad;

                QuestPDF.Settings.License = LicenseType.Community;

                string ruta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Cita_{nombrePaciente}_{cita.Fecha}.pdf");

                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Column(col =>
                        {

                            // Encabezado con logo
                            var logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                "Assets", "Logo_Black.png");

                            if (File.Exists(logoPath))
                            {
                                col.Item().Width(200).Image(logoPath).FitWidth();
                            }
                            else
                            {
                                col.Item().Text("Clínica Lorenzo")
                                    .FontSize(28).Bold().FontColor("#001e3b");
                            }

                            col.Item().AlignCenter().Text("Comprobante de Cita Médica")
                                .FontSize(14).FontColor("#4879AB");

                            col.Item().PaddingTop(8).AlignCenter()
                                .Text("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                                .FontColor("#4879AB");

                            col.Item().PaddingTop(16).Text("INFORMACIÓN DEL PACIENTE")
                                .FontSize(13).Bold().FontColor("#001e3b");

                            col.Item().PaddingTop(4)
                                .Text("──────────────────────────────────────────────────")
                                .FontColor("#cccccc");

                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.ConstantItem(150).Text("Nombre:").Bold();
                                row.RelativeItem().Text(nombrePaciente);
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Tipo de sangre:").Bold();
                                row.RelativeItem().Text(paciente?.TipoSangre ?? "-");
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Alergias:").Bold();
                                row.RelativeItem().Text(paciente?.Alergias ?? "-");
                            });

                            col.Item().PaddingTop(16).Text("INFORMACIÓN DE LA CITA")
                                .FontSize(13).Bold().FontColor("#001e3b");

                            col.Item().PaddingTop(4)
                                .Text("──────────────────────────────────────────────────")
                                .FontColor("#cccccc");

                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.ConstantItem(150).Text("Doctor:").Bold();
                                row.RelativeItem().Text(nombreDoctor);
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Especialidad:").Bold();
                                row.RelativeItem().Text(especialidad);
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Fecha:").Bold();
                                row.RelativeItem().Text(cita.Fecha);
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Hora:").Bold();
                                row.RelativeItem().Text(cita.Hora);
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Motivo:").Bold();
                                row.RelativeItem().Text(cita.Motivo);
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(150).Text("Estado:").Bold();
                                row.RelativeItem().Text(cita.Estado);
                            });

                            col.Item().PaddingTop(24)
                                .Text("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                                .FontColor("#4879AB");

                            col.Item().PaddingTop(8).AlignCenter()
                                .Text("Este documento es un comprobante oficial de su cita médica.")
                                .FontSize(10).FontColor("#888888").Italic();

                            col.Item().AlignCenter()
                                .Text($"Generado el {DateTime.Today:dd/MM/yyyy}")
                                .FontSize(10).FontColor("#888888");
                        });
                    });
                }).GeneratePdf(ruta);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}