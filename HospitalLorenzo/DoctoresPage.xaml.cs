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

namespace HospitalLorenzo
{
    public sealed partial class DoctoresPage : Page
    {
        private static readonly string DataPath = Rutas.Doctores;
        private List<Doctor> _listaDoctoresMemoria = new();
        private List<string> _especialidades = new();

        public DoctoresPage()
        {
            this.InitializeComponent();
            CargarDirectorioMedico();
            _ = CargarEspecialidadesAsync();
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

        private async Task CargarEspecialidadesAsync()
        {
            try
            {
                if (!File.Exists(Rutas.Especialidades)) return;
                string json = await File.ReadAllTextAsync(Rutas.Especialidades);
                _especialidades = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                cmbEspecialidad.ItemsSource = _especialidades;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error cargando especialidades: {ex.Message}");
            }
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
            => Regex.IsMatch(Cedula, @"^\d{8}$");

        public static bool ValidarTelefono(string Telefono)
            => Regex.IsMatch(Telefono, @"^\d{10}$");

        private void txtCedula_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtCedula.Text;
            bool soloNumeros = string.IsNullOrWhiteSpace(texto) || Regex.IsMatch(texto, @"^\d+$");

            if (string.IsNullOrWhiteSpace(texto))
            {
                EstadoCedula.Text = "La cédula es obligatoria.";
                EstadoCedula.Visibility = Visibility.Visible;
            }
            else if (!soloNumeros)
            {
                EstadoCedula.Text = "La cédula solo debe contener números.";
                EstadoCedula.Visibility = Visibility.Visible;
            }
            else if (texto.Length != 8)
            {
                EstadoCedula.Text = "La cédula debe tener exactamente 8 dígitos.";
                EstadoCedula.Visibility = Visibility.Visible;
            }
            else
            {
                EstadoCedula.Visibility = Visibility.Collapsed;
            }
        }

        private void txtTelefono_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtTelefono.Text;
            bool soloNumeros = string.IsNullOrWhiteSpace(texto) || Regex.IsMatch(texto, @"^\d+$");

            if (string.IsNullOrWhiteSpace(texto))
            {
                EstadoTelefono.Text = "El teléfono es obligatorio.";
                EstadoTelefono.Visibility = Visibility.Visible;
            }
            else if (!soloNumeros)
            {
                EstadoTelefono.Text = "El teléfono solo debe contener números.";
                EstadoTelefono.Visibility = Visibility.Visible;
            }
            else if (texto.Length != 10)
            {
                EstadoTelefono.Text = "El teléfono debe tener exactamente 10 dígitos.";
                EstadoTelefono.Visibility = Visibility.Visible;
            }
            else
            {
                EstadoTelefono.Visibility = Visibility.Collapsed;
            }
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtNombreDoc.Text;
            bool soloLetras = string.IsNullOrWhiteSpace(texto) ||
                              Regex.IsMatch(texto.Replace(" ", ""), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+$");

            if (string.IsNullOrWhiteSpace(texto))
            {
                ErrorNombreDoc.Text = "El nombre es obligatorio.";
                ErrorNombreDoc.Visibility = Visibility.Visible;
            }
            else if (!soloLetras)
            {
                ErrorNombreDoc.Text = "Solo se permiten letras, sin números ni símbolos.";
                ErrorNombreDoc.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorNombreDoc.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnGuardarDoctor_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombreDoc.Text?.Trim();
            var cedula = txtCedula.Text?.Trim();
            var telefono = txtTelefono.Text?.Trim();

            if (ErrorNombreDoc.Visibility == Visibility.Visible)
            {
                MostrarMensaje("El nombre es obligatorio.");
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
            int nuevoId = data.Doctores.Count > 0 ? data.Doctores.Max(d => d.Id) + 1 : 1;

            var nuevoDoc = new Doctor
            {
                Id = nuevoId,
                Nombre = nombre,
                Especialidad = cmbEspecialidad.SelectedItem?.ToString() ?? "General",
                Cedula = cedula,
                Telefono = telefono,
                Activo = true,
                Disponible = true
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
                lblDetalleDoctor.Text =
                    $"Especialidad: {d.Especialidad}\n" +
                    $"Cédula: {d.Cedula}\n" +
                    $"Teléfono: {d.Telefono}\n" +
                    $"Estado: {(d.Activo ? "Activo" : "Inactivo")}\n" +
                    $"Disponibilidad: {(d.Disponible ? "Disponible" : "No disponible")}";
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
            txtTelefono.Text = "";

            cmbEspecialidad.SelectedIndex = -1;

            EstadoCedula.Text = "";
            EstadoTelefono.Text = "";

            EstadoCedula.Visibility = Visibility.Collapsed;
            EstadoTelefono.Visibility = Visibility.Collapsed;
            ErrorNombreDoc.Visibility = Visibility.Collapsed;

            txtEstado.Text = "";
        }
    }
}