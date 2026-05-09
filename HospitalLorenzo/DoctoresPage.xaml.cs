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

        private async void BtnGuardarDoctor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombreDoc.Text)) { return; }

                var data = await CargarDataAsync();

                int nuevoId = data.Doctores.Count > 0 ? data.Doctores.Max(d => d.Id) + 1 : 1;

                var nuevoDoc = new Doctor
                {
                    Id = nuevoId,
                    Nombre = txtNombreDoc.Text,
                    Especialidad = (cmbEspecialidad.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "General",
                    Cedula = txtCedula.Text,
                    Telefono = txtTelefono.Text ?? "",
                    Turno = (cmbTurno.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                    Activo = true
                };

                data.Doctores.Add(nuevoDoc);
                await GuardarDataAsync(data);

                BtnCancelarRegistro_Click(this, new RoutedEventArgs());
                CargarDirectorioMedico();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}", true);
            }
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

        private void BtnEditarDoctor_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            PanelDetalleDoctor.Visibility = Visibility.Collapsed;
            ListaDoctores.SelectedItem = null;
        }

        private void MostrarMensaje(string mensaje, bool isError)
        {
        }

        private void LimpiarFormularioDoctor()
        {
            txtNombreDoc.Text = "";
            txtCedula.Text = "";
            cmbEspecialidad.SelectedIndex = -1;
        }
    }
}
