using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalLorenzo
{
    public sealed partial class ReportesPage : Page
    {
        private List<Cita> _citasSemana = new();

        public ReportesPage()
        {
            InitializeComponent();
            CargarReporte();
        }

        private async void CargarReporte()
        {
            var citas = await LeerCitas();

            var hoy = DateTime.Today;
            int diff = (7 + (int)hoy.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var inicioSemana = hoy.AddDays(-diff);
            var finSemana = inicioSemana.AddDays(6);

            var citasSemana = citas.Where(c =>
            {
                if (DateTime.TryParse(c.Fecha, out var fecha))
                    return fecha >= inicioSemana && fecha <= finSemana;
                return false;
            }).ToList();

            TxtTotalCitas.Text = citasSemana.Count.ToString();
            TxtTotalPacientes.Text = citasSemana.Select(c => c.PacienteId).Distinct().Count().ToString();
            TxtTotalDoctores.Text = citasSemana.Select(c => c.DoctorId).Distinct().Count().ToString();

            _citasSemana = citasSemana;
            ListaCitasSemana.ItemsSource = _citasSemana;
        }

        private async Task<List<Cita>> LeerCitas()
        {
            try
            {
                string path = Rutas.Citas;
                if (!File.Exists(path)) return new List<Cita>();
                string json = await File.ReadAllTextAsync(path);
                var data = JsonSerializer.Deserialize<CitasData>(json);
                return data?.Citas ?? new List<Cita>();
            }
            catch { return new List<Cita>(); }
        }

        private void BtnDescargarPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var citas = _citasSemana;

                if (citas == null || citas.Count == 0) return;

                QuestPDF.Settings.License = LicenseType.Community;

                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ReporteSemanal.pdf");

                var hoy = DateTime.Today;
                int diff = (7 + (int)hoy.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var inicioSemana = hoy.AddDays(-diff);

                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header().Column(header =>
                        {
                            var logoPath = System.IO.Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                "Assets", "Logo_Black.png");

                            if (File.Exists(logoPath))
                                header.Item().Width(200).Image(logoPath).FitWidth();
                            else
                                header.Item().Text("Clínica Lorenzo")
                                    .FontSize(28).Bold().FontColor("#001e3b");

                            header.Item().Text("Reporte Semanal")
                                .FontSize(16).FontColor("#4879AB");

                            header.Item().Text("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                                .FontColor("#4879AB");
                        });

                        page.Content().Column(col =>
                        {
                            col.Item().Text($"Semana del {inicioSemana:dd/MM/yyyy}")
                                .FontSize(12).FontColor("#888888");

                            col.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text($"Total citas: {citas.Count}").Bold();
                                row.RelativeItem().Text($"Total pacientes: {citas.Select(c => c.PacienteId).Distinct().Count()}").Bold();
                            });

                            col.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); 
                                    columns.RelativeColumn();   
                                    columns.RelativeColumn();  
                                    columns.RelativeColumn();  
                                    columns.ConstantColumn(100); 
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#001e3b").Padding(5).Text("Folio Cita").FontColor("#ffffff").Bold().FontSize(12);
                                    header.Cell().Background("#001e3b").Padding(5).Text("Paciente").FontColor("#ffffff").Bold().FontSize(12);
                                    header.Cell().Background("#001e3b").Padding(5).Text("Doctor").FontColor("#ffffff").Bold().FontSize(12);
                                    header.Cell().Background("#001e3b").Padding(5).Text("Fecha y Hora").FontColor("#ffffff").Bold().FontSize(12);
                                    header.Cell().Background("#001e3b").Padding(5).Text("Estado").FontColor("#ffffff").Bold().FontSize(12);
                                });

                                foreach (var cita in citas)
                                {
                                    table.Cell().Padding(4).Text(cita.Id.ToString());
                                    table.Cell().Padding(4).Text(cita.PacienteNombre);
                                    table.Cell().Padding(4).Text(cita.DoctorNombre);
                                    table.Cell().Padding(4).Text($"{cita.Fecha} {cita.Hora}");
                                    table.Cell().Padding(4).Text(cita.Estatus);
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(ruta);

                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}