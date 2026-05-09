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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HospitalLorenzo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    
    public sealed partial class DoctoresPage : Page
    {

        private static readonly string DataPath = Rutas.Doctores;  
        private List<Doctor> _listaDoctoresMemoria = new();

        public DoctoresPage()
        {
            this.InitializeComponent();
            CargarDirectorioMedico();
        }

        private static async Task<DoctoresData> CargarDataAsync()
        {
            try
            {
                if (!File.Exists(DataPath)) return new DoctoresData();
                string json = await File.ReadAllTextAsync(DataPath);
                return JsonSerializer.Deserialize<DoctoresData>(json) ?? new DoctoresData();
            }
            catch { return new DoctoresData(); }
        }

        private static async Task GuardarDataAsync(DoctoresData data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(DataPath, json);
        }

        private async void CargarDirectorioMedico()
        {
            var data = await CargarDataAsync();
            _listaDoctoresMemoria = data.Doctores;
            ListaDoctores.ItemsSource = _listaDoctoresMemoria;
        }

        private void BtnNuevoDoctor_Click(object sender, RoutedEventArgs e)
        {
            SeccionBusqueda.Visibility = Visibility.Collapsed;
            SeccionRegistroDoctor.Visibility = Visibility.Visible;
        }

        private void BtnCancelarRegistro_Click(object sender, RoutedEventArgs e)
        {
            SeccionRegistroDoctor.Visibility = Visibility.Collapsed;
            SeccionBusqueda.Visibility = Visibility.Visible;
            LimpiarFormularioDoctor();
        }

        public static bool ValidarCedulaProfesional(string Cedula)
        {
            return Regex.IsMatch(Cedula, @"^\d{8}$");
        }

        public static bool ValidarTelefono(string Telefono)
        {
            return Regex.IsMatch(Telefono, @"^\d{10}$");
        }

        private void txtCedula_TextChanged(object sender, TextChangedEventArgs e)
        {
            string cedula = txtCedula.Text;

            if (ValidarCedulaProfesional(cedula))
            {
                EstadoCedula.Text = "✓ Cédula válida";
                EstadoCedula.Foreground =
                    new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                EstadoCedula.Text = "✗ La cédula debe tener 8 dígitos";
                EstadoCedula.Foreground =
                    new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private void txtTelefono_TextChanged(object sender, TextChangedEventArgs e)
        {
            string telefono = txtTelefono.Text;

            if (ValidarTelefono(telefono))
            {
                EstadoTelefono.Text = "✓ Teléfono válido";
                EstadoTelefono.Foreground =
                    new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                EstadoTelefono.Text = "✗ El teléfono debe tener 10 dígitos";
                EstadoTelefono.Foreground =
                    new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private async void BtnGuardarDoctor_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombreDoc.Text?.Trim();
            var cedula = txtCedula.Text?.Trim();
            var telefono = txtTelefono.Text?.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                MostrarMensaje("Ingresa el nombre del doctor.", true);
                return;
            }

            if (!ValidarCedulaProfesional(cedula))
            {
                MostrarMensaje("La cédula debe tener exactamente 8 dígitos.", true);
                return;
            }

            if (!ValidarTelefono(telefono))
            {
                MostrarMensaje("El teléfono debe tener exactamente 10 dígitos.", true);
                return;
            }

            var data = await CargarDataAsync();

            int nuevoId = data.Doctores.Count > 0
                ? data.Doctores.Max(d => d.Id) + 1
                : 1;

            var nuevoDoc = new Doctor
            {
                Id = nuevoId,
                Nombre = nombre,
                Especialidad = (cmbEspecialidad.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "General",
                Cedula = cedula,
                Telefono = telefono,
                Turno = (cmbTurno.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                Activo = true
            };

            data.Doctores.Add(nuevoDoc);

            await GuardarDataAsync(data);

            MostrarMensaje("Doctor registrado correctamente.", false);

            BtnCancelarRegistro_Click(this, new RoutedEventArgs());

            CargarDirectorioMedico();
        }


        private async void TxtBusquedaDoctor_TextChanged(object sender, TextChangedEventArgs e)
        {
            string busqueda = txtBusquedaDoctor.Text.ToLower();
            var data = await CargarDataAsync();

            ListaDoctores.ItemsSource = data.Doctores
                .Where(d => d.Id.ToString().Contains(busqueda))
                .ToList();
        }

        private void ListaDoctores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaDoctores.SelectedItem is Doctor d)
            {
                PanelDetalleDoctor.Visibility = Visibility.Visible;
                lblNombreDoctor.Text = $"ID: {d.Id} - Dr. {d.Nombre}";
                lblDetalleDoctor.Text = $"Especialidad: {d.Especialidad}\n" +
                                        $"Cédula: {d.Cedula}\n" +
                                        $"Teléfono: {d.Telefono}\n" +
                                        $"Turno: {d.Turno}\n" +
                                        $"Estado: {(d.Activo ? "Activo" : "Inactivo")}";
            }
        }


        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            PanelDetalleDoctor.Visibility = Visibility.Collapsed;
            ListaDoctores.SelectedItem = null;
        }

        private void MostrarMensaje(string mensaje, bool isError = true)
        {
            txtEstado.Text = mensaje;

            txtEstado.Foreground = isError
                ? new SolidColorBrush(Microsoft.UI.Colors.Red)
                : new SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        private void LimpiarFormularioDoctor()
        {
            txtNombreDoc.Text = "";
            txtCedula.Text = "";
            cmbEspecialidad.SelectedIndex = -1;
        }
    }
}
