using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HospitalLorenzo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>


    public sealed partial class PacientesPage : Page
    {
        private static readonly string DataPath = Path.Combine(AppContext.BaseDirectory, "pacientes.json");
        private List<Paciente> _listaEnMemoria = new();

        private static async Task<PacientesData> CargarDataAsync()
        {
            try
            {
                if (!File.Exists(DataPath)) return new PacientesData();
                string json = await File.ReadAllTextAsync(DataPath);
                return JsonSerializer.Deserialize<PacientesData>(json) ?? new PacientesData();
            }
            catch { return new PacientesData(); }
        }

        public PacientesPage()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(Path.GetDirectoryName(DataPath)!);
        }

        private static async Task GuardarDataAsync(PacientesData data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(DataPath, json);
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MostrarMensaje("El nombre es obligatorio.", true);
                    return;
                }

                var data = await CargarDataAsync();
                int nuevoId = data.Pacientes.Count > 0
                              ? data.Pacientes.Max(p => p.Id) + 1
                              : 1;

                var nuevo = new Paciente
                {
                    Id = nuevoId,
                    Nombre = $"{txtNombre.Text} {txtApellido.Text}".Trim(),
                    FechaNacimiento = datePickerNac.Text ?? "No especificada",
                    Sexo = (comboSexo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No especificado",
                    TipoSangre = (comboSangre.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No especificado",
                    Telefono = txtTelefono.Text ?? "",
                    Correo = txtCorreo.Text ?? "",
                    Direccion = txtDireccion.Text ?? "",
                    Alergias = txtAlergias.Text ?? "",
                    Enfermedades = txtCronicas.Text ?? "",
                    FechaRegistro = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Activo = true
                };

                var datos = await CargarDataAsync();

                data.Pacientes.Add(nuevo);
                await GuardarDataAsync(data);

                MostrarMensaje($"Paciente guardado correctamente. ID: {nuevoId}", false);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}", true);
            }
        }


        private async void BtnVerExpediente_Click(object sender, RoutedEventArgs e)
        {
            FormularioRegistro.Visibility = Visibility.Collapsed;
            SeccionExpedientes.Visibility = Visibility.Visible;

            var data = await CargarDataAsync();

            _listaEnMemoria = data.Pacientes;
            TablaReporte.ItemsSource = _listaEnMemoria;
        }

        private void BtnNuevoPaciente_Click(object sender, RoutedEventArgs e)
        {
            SeccionExpedientes.Visibility = Visibility.Collapsed;
            FormularioRegistro.Visibility = Visibility.Visible;
        }


        private async void TxtFiltroID(object sender, TextChangedEventArgs e)
        {
            string busqueda = (sender as TextBox)?.Text ?? "";
            var data = await CargarDataAsync();

            TablaReporte.ItemsSource = data.Pacientes
                .Where(p => p.Id.ToString().Contains(busqueda))
                .ToList();
        }

        private async void BtnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            var data = await CargarDataAsync();
            TablaReporte.ItemsSource = data.Pacientes;
        }


        private void TablaExpediente(object sender, SelectionChangedEventArgs e)
        {
            if (TablaReporte.SelectedItem is Paciente p)
            {
                PanelDetalle.Visibility = Visibility.Visible;
                lblDetalleNombre.Text = $"ID: {p.Id} - {p.Nombre}";
                lblDetalleTodo.Text =
                    $"Nacimiento: {p.FechaNacimiento}\n" +
                    $"Sexo: {p.Sexo}\n" +
                    $"Tipo de Sangre: {p.TipoSangre}\n" +
                    $"Teléfono: {p.Telefono}\n" +
                    $"Correo: {p.Correo}\n" +
                    $"Dirección: {p.Direccion}\n" +
                    $"Alergias: {p.Alergias}\n" +
                    $"Enfermedades: {p.Enfermedades}\n" +
                    $"Registro: {p.FechaRegistro}\n" +
                    $"Estado: {(p.Activo ? "Activo" : "Inactivo")}";
            }
        }

        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            PanelDetalle.Visibility = Visibility.Collapsed;
            TablaReporte.SelectedItem = null;
        }


        private void MostrarMensaje(string mensaje, bool isError)
        {
            txtMensaje.Text = mensaje;
            txtMensaje.Foreground = isError
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = txtApellido.Text = txtDireccion.Text = "";
            txtTelefono.Text = txtCorreo.Text = txtAlergias.Text = txtCronicas.Text = "";
            datePickerNac.Text = "";
            comboSexo.SelectedIndex = -1;
            comboSangre.SelectedIndex = -1;
        }
    }
}
