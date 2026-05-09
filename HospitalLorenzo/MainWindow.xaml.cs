using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalLorenzo
{
    public sealed partial class MainWindow : Window
    {
        private static readonly string UsersFilePath =
            Path.Combine(AppContext.BaseDirectory, "usuarios.json");

        public MainWindow()
        {
            this.InitializeComponent();
            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
        private void fileExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public record User(string Username, string Password);

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var username = UsernameTextBox.Text?.Trim();
                var password = PasswordBox.Password ?? string.Empty;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    SetStatus("Ingresa usuario y contraseña.");
                    return;
                }

                var users = await LoadUsersAsync();
                var match = users.FirstOrDefault(u =>
                    string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)
                    && u.Password == password);

                if (match is null)
                {
                    SetStatus("Usuario o contraseña incorrecto.");
                    return;
                }

                SetStatus("Sesión iniciada.", isError: false);

                var menu = new MenuWindow();
                menu.Activate();
                this.Close();
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
        }


        private bool ValidarContrasena(string password)
        {
            if (password.Length < 3)
            {
                SetStatus("La contraseña debe tener al menos 3 caracteres.");
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                SetStatus("La contraseña debe tener al menos una mayúscula.");
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                SetStatus("La contraseña debe tener al menos un número.");
                return false;
            }

            if (!password.Any(ch => "!@#$%^&*()_+-=[]{}".Contains(ch)))
            {
                SetStatus("La contraseña debe tener al menos un carácter especial (!@#$...).");
                return false;
            }

            return true;
        }



            
        private async void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text?.Trim();
            var password = PasswordBox.Password ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                SetStatus("Ingresa usuario y contraseña.");
                return;
            }

            if (!ValidarContrasena(password))
                return;

            var users = await LoadUsersAsync();
            if (users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                SetStatus("El usuario ya existe.");
                return;
            }

            users.Add(new User(username, password));
            await SaveUsersAsync(users);
            SetStatus("Usuario creado.", isError: false);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ReglasPanel.Visibility = Visibility.Visible;
            var password = PasswordBox.Password;

            ActualizarRegla(ReglaTamano, password.Length >= 3, "Mínimo 3 caracteres");
            ActualizarRegla(ReglaMayuscula, password.Any(char.IsUpper), "Al menos una mayúscula");
            ActualizarRegla(ReglaNumero, password.Any(char.IsDigit), "Al menos un número");
            ActualizarRegla(ReglaEspecial, password.Any(ch => "!@#$%^&*()_+-=[]{}".Contains(ch)), "Al menos un carácter especial (!@#$...)");
        }

        private void ActualizarRegla(TextBlock bloque, bool cumple, string texto)
        {
            if (cumple)
            {
                bloque.Text = $"✓ {texto}";
                bloque.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 72, 121, 171)); // verde
            }
            else
            {
                bloque.Text = $"✗ {texto}";
                bloque.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)); // rojo
            }
        }   

        private void SetStatus(string message, bool isError = true)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isError
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        private static async Task<List<User>> LoadUsersAsync()
        {
            if (!File.Exists(UsersFilePath)) return new List<User>();
            var json = await File.ReadAllTextAsync(UsersFilePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private static async Task SaveUsersAsync(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(UsersFilePath, json);
        }
    }
}