using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HospitalLorenzo
{
    public sealed partial class PacientesPage : Page
    {
        private static readonly string DataPath = Rutas.Pacientes;
        private List<Paciente> _listaEnMemoria = new();

        public PacientesPage()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(Path.GetDirectoryName(DataPath)!);
        }

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

        private static async Task GuardarDataAsync(PacientesData data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(DataPath, json);
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtNombre.Text;
            bool soloLetras = string.IsNullOrWhiteSpace(texto) ||
                              Regex.IsMatch(texto.Replace(" ", ""), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+$");

            if (string.IsNullOrWhiteSpace(texto))
            {
                ErrorNombre.Text = "El nombre es obligatorio.";
                ErrorNombre.Visibility = Visibility.Visible;
            }
            else if (!soloLetras)
            {
                ErrorNombre.Text = "Solo se permiten letras, sin números ni símbolos.";
                ErrorNombre.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorNombre.Visibility = Visibility.Collapsed;
            }
        }

        private void txtApellido_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtApellido.Text;
            bool soloLetras = string.IsNullOrWhiteSpace(texto) ||
                              Regex.IsMatch(texto.Replace(" ", ""), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+$");

            if (string.IsNullOrWhiteSpace(texto))
            {
                ErrorApellido.Text = "El apellido es obligatorio.";
                ErrorApellido.Visibility = Visibility.Visible;
            }
            else if (!soloLetras)
            {
                ErrorApellido.Text = "Solo se permiten letras, sin números ni símbolos.";
                ErrorApellido.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorApellido.Visibility = Visibility.Collapsed;
            }
        }

        private void txtDireccion_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtDireccion.Text;
            bool caracteresValidos = string.IsNullOrWhiteSpace(texto) ||
                                     Regex.IsMatch(texto, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s,.\-/#]+$");

            if (!string.IsNullOrWhiteSpace(texto) && !caracteresValidos)
            {
                ErrorDireccion.Text = "Caracteres no permitidos en la dirección.";
                ErrorDireccion.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorDireccion.Visibility = Visibility.Collapsed;
            }
        }

        private void txtTelefono_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtTelefono.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                ErrorTelefono.Text = "El teléfono es obligatorio.";
                ErrorTelefono.Visibility = Visibility.Visible;
            }
            else if (!texto.All(char.IsDigit))
            {
                ErrorTelefono.Text = "Solo se permiten números.";
                ErrorTelefono.Visibility = Visibility.Visible;
            }
            else if (texto.Length != 10)
            {
                ErrorTelefono.Text = $"El teléfono debe tener exactamente 10 dígitos ({texto.Length}/10).";
                ErrorTelefono.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorTelefono.Visibility = Visibility.Collapsed;
            }
        }

        private void txtCorreo_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtCorreo.Text.ToLower();
            string[] dominiosValidos = { "@gmail.com", "@yahoo.com", "@icloud.com", "@hotmail.com", "@outlook.com" };

            bool tieneCaracterAntes = texto.Contains("@") && texto.IndexOf("@") > 0;
            bool dominioValido = dominiosValidos.Any(d => texto.EndsWith(d));

            if (string.IsNullOrWhiteSpace(texto))
            {
                ErrorCorreo.Text = "El correo es obligatorio.";
                ErrorCorreo.Visibility = Visibility.Visible;
            }
            else if (!tieneCaracterAntes)
            {
                ErrorCorreo.Text = "Debe haber al menos un carácter antes del @.";
                ErrorCorreo.Visibility = Visibility.Visible;
            }
            else if (!dominioValido)
            {
                ErrorCorreo.Text = "Solo: @gmail.com @yahoo.com @icloud.com @hotmail.com @outlook.com";
                ErrorCorreo.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorCorreo.Visibility = Visibility.Collapsed;
            }
        }

        private void comboSexo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboSexo.SelectedItem == null)
            {
                ErrorSexo.Text = "Selecciona un género.";
                ErrorSexo.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorSexo.Visibility = Visibility.Collapsed;
            }
        }

        private void comboSangre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboSangre.SelectedItem == null)
            {
                ErrorSangre.Text = "Selecciona un tipo de sangre.";
                ErrorSangre.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorSangre.Visibility = Visibility.Collapsed;
            }
        }

        private bool FormularioValido()
        {
            return ErrorNombre.Visibility == Visibility.Collapsed &&
                   ErrorApellido.Visibility == Visibility.Collapsed &&
                   ErrorDireccion.Visibility == Visibility.Collapsed &&
                   ErrorTelefono.Visibility == Visibility.Collapsed &&
                   ErrorCorreo.Visibility == Visibility.Collapsed &&
                   ErrorSexo.Visibility == Visibility.Collapsed &&
                   ErrorSangre.Visibility == Visibility.Collapsed;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            txtNombre_TextChanged(null, null);
            txtApellido_TextChanged(null, null);
            txtDireccion_TextChanged(null, null);
            txtTelefono_TextChanged(null, null);
            txtCorreo_TextChanged(null, null);

            if (!FormularioValido())
            {
                MostrarMensaje("Corrige los errores antes de guardar.", true);
                return;
            }

            try
            {
                var data = await CargarDataAsync();
                int nuevoId = data.Pacientes.Count > 0
                              ? data.Pacientes.Max(p => p.Id) + 1 : 1;

                var nuevo = new Paciente
                {
                    Id = nuevoId,
                    Nombre = $"{txtNombre.Text.Trim()} {txtApellido.Text.Trim()}",
                    FechaNacimiento = datePickerNac.Date.HasValue
                                      ? datePickerNac.Date.Value.ToString("dd/MM/yyyy")
                                      : "No especificada",
                    Sexo = (comboSexo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No especificado",
                    TipoSangre = (comboSangre.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No especificado",
                    Telefono = $"+52{txtTelefono.Text}", 
                    Correo = txtCorreo.Text,
                    Direccion = txtDireccion.Text ?? "",
                    Alergias = txtAlergias.Text ?? "",
                    Enfermedades = txtCronicas.Text ?? "",
                    FechaRegistro = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Activo = true
                };

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
                ? new SolidColorBrush(Microsoft.UI.Colors.Red)
                : new SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = txtApellido.Text = txtDireccion.Text = "";
            txtTelefono.Text = txtCorreo.Text = txtAlergias.Text = txtCronicas.Text = "";
            datePickerNac.Date = null;
            comboSexo.SelectedIndex = -1;
            comboSangre.SelectedIndex = -1;
        }
    }
}