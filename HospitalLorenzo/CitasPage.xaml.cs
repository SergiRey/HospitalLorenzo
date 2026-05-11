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
            var pacientes = await LeerPacientesDesdeJson();
            cbPacientes.ItemsSource = pacientes;

            var doctores = await LeerDoctoresDesdeJson();
            cbDoctores.ItemsSource = doctores;

            _todasLasCitas = await LeerCitasDesdeJson();
            ListaCitas.ItemsSource = _todasLasCitas;
        }

        private async Task<List<Paciente>> LeerPacientesDesdeJson()
        {
            string path = Rutas.Pacientes;
            if (!File.Exists(path)) return new List<Paciente>();
            string json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<PacientesData>(json);
            return data?.Pacientes ?? new List<Paciente>();
        }

        private async Task<List<Doctor>> LeerDoctoresDesdeJson()
        {
            string path = Rutas.Doctores;
            if (!File.Exists(path)) return new List<Doctor>();
            string json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<DoctoresData>(json);
            return data?.Doctores ?? new List<Doctor>();
        }

        private async Task<List<Cita>> LeerCitasDesdeJson()
        {
            var data = await CargarCitasAsync();
            return data.Citas ?? new List<Cita>();
        }

        private async Task<CitasData> CargarCitasAsync()
        {
            try
            {
                if (!File.Exists(DataPathCitas)) return new CitasData();
                string json = await File.ReadAllTextAsync(DataPathCitas);
                return JsonSerializer.Deserialize<CitasData>(json) ?? new CitasData();
            }
            catch { return new CitasData(); }
        }

        private async Task GuardarCitasAsync(CitasData data)
        {
            Rutas.AsegurarDirectorio(Rutas.Citas);
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
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

        
        private void CmbEstatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb && cmb.Tag is int id)
            {
                var cita = _todasLasCitas.FirstOrDefault(c => c.Id == id);
                if (cita != null && cmb.SelectedItem is ComboBoxItem item)
                {
                    string nuevoEstatus = item.Content.ToString() ?? "Programada";
                    cita.Estatus = nuevoEstatus;
                    cita.Estado = nuevoEstatus; 
                }
            }
        }

        private async void BtnGuardarEstatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var cita = _todasLasCitas.FirstOrDefault(c => c.Id == id);
                if (cita != null)
                {
                    await GuardarCitasAsync(new CitasData { Citas = _todasLasCitas });

                    ListaCitas.ItemsSource = null;
                    ListaCitas.ItemsSource = _todasLasCitas;
                }
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {

            var errores = new List<string>();

            if (cbPacientes.SelectedItem == null)
                errores.Add("Debes seleccionar un paciente.");

            if (cbDoctores.SelectedItem == null)
                errores.Add("Debes seleccionar un médico.");

            if (!dpFechaCita.Date.HasValue)
                errores.Add("Debes elegir una fecha para la cita.");
            else if (dpFechaCita.Date.Value.Date < DateTime.Today)
                errores.Add("La fecha no puede ser anterior a hoy.");

            if (string.IsNullOrWhiteSpace(txtMotivo.Text))
                errores.Add("El motivo de la cita es obligatorio.");

            if (errores.Count > 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Datos incompletos",
                    Content = string.Join("\n", errores),
                    CloseButtonText = "Entendido",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            if (cbPacientes.SelectedItem == null || cbDoctores.SelectedItem == null)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Selecciona paciente y doctor");
                await dialog.ShowAsync();
                return;
            }

            var pacienteSeleccionado = cbPacientes.SelectedItem as Paciente;
            var doctorSeleccionado = cbDoctores.SelectedItem as Doctor;

            var nuevaCita = new Cita
            {
                Id = _todasLasCitas.Count > 0 ? _todasLasCitas.Max(c => c.Id) + 1 : 1,
                PacienteId = pacienteSeleccionado?.Id ?? 0,
                DoctorId = doctorSeleccionado?.Id ?? 0,
                PacienteNombre = pacienteSeleccionado?.Nombre ?? "Sin nombre",
                DoctorNombre = doctorSeleccionado?.Nombre ?? "Sin nombre",
                Motivo = txtMotivo.Text,
                Fecha = dpFechaCita.Date.HasValue
                                 ? dpFechaCita.Date.Value.ToString("yyyy-MM-dd")
                                 : DateTime.Today.ToString("yyyy-MM-dd"),
                Hora = tpHora.Time.Hours.ToString("00") + ":" + tpHora.Time.Minutes.ToString("00"),
                Estatus = "Programada",
                Estado = "Programada"
            };

            _todasLasCitas.Add(nuevaCita);
            await GuardarCitasAsync(new CitasData { Citas = _todasLasCitas });

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

                var pacientes = await LeerPacientesDesdeJson();
                var doctores = await LeerDoctoresDesdeJson();

                var paciente = pacientes.FirstOrDefault(p => p.Id == cita.PacienteId);
                var doctor = doctores.FirstOrDefault(d => d.Id == cita.DoctorId);

                string nombrePaciente = paciente?.Nombre ?? cita.PacienteNombre ?? "Sin nombre";
                string nombreDoctor = doctor?.Nombre ?? cita.DoctorNombre ?? "Sin nombre";
                string especialidad = doctor?.Especialidad ?? "-";
                string cedulaDoctor = doctor?.Cedula ?? "-";

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
                            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Logo_Black.png");
                            if (File.Exists(logoPath))
                                col.Item().Width(200).Image(logoPath).FitWidth();
                            else
                                col.Item().Text("Clínica Lorenzo").FontSize(28).Bold().FontColor("#001e3b");

                            col.Item().AlignCenter().Text("Comprobante de Cita Médica")
                                .FontSize(14).FontColor("#4879AB");

                            col.Item().PaddingTop(4).AlignCenter()
                                .Text($"Folio: {cita.Id}")
                                .FontSize(11).FontColor("#7a8fa6");

                            col.Item().PaddingTop(12).PaddingBottom(4)
                                .LineHorizontal(1).LineColor("#4879AB");

                            col.Item().PaddingTop(12)
                                .Text("INFORMACIÓN DEL PACIENTE")
                                .FontSize(13).Bold().FontColor("#001e3b");

                            void Fila(string etiqueta, string valor)
                            {
                                col.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(160).Text(etiqueta).Bold();
                                    row.RelativeItem().Text(string.IsNullOrWhiteSpace(valor) ? "-" : valor);
                                });
                            }

                            Fila("Nombre:", nombrePaciente);
                            Fila("Fecha de nacimiento:", paciente?.FechaNacimiento ?? "-");
                            Fila("Sexo:", paciente?.Sexo ?? "-");
                            Fila("Teléfono:", paciente?.Telefono ?? "-");
                            Fila("Correo:", paciente?.Correo ?? "-");
                            Fila("Dirección:", paciente?.Direccion ?? "-");
                            Fila("Tipo de sangre:", paciente?.TipoSangre ?? "-");
                            Fila("Alergias:", paciente?.Alergias ?? "-");
                            Fila("Enfermedades crónicas:", paciente?.Enfermedades ?? "-");

                            col.Item().PaddingTop(12).PaddingBottom(4)
                                .LineHorizontal(1).LineColor("#4879AB");

                            col.Item().PaddingTop(12)
                                .Text("INFORMACIÓN DE LA CITA")
                                .FontSize(13).Bold().FontColor("#001e3b");

                            Fila("Doctor:", nombreDoctor);
                            Fila("Especialidad:", especialidad);
                            Fila("Cédula:", cedulaDoctor);
                            Fila("Fecha:", cita.Fecha);
                            Fila("Hora:", cita.Hora);
                            Fila("Motivo:", cita.Motivo);
                            Fila("Estado:", cita.Estado);

                            col.Item().PaddingTop(24).AlignCenter()
                                .Text($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9).FontColor("#aaaaaa");
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