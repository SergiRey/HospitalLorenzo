using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Media.Capture.Core;


namespace HospitalLorenzo
{

    public static class Rutas
    {
        private static readonly string RutaBase = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\.."));
        public static string Usuarios => Path.Combine(RutaBase, "JSON", "usuarios.json");
        public static string Doctores => Path.Combine(RutaBase, "JSON", "doctores.json");
        public static string Pacientes => Path.Combine(RutaBase, "JSON", "pacientes.json");
        public static string Citas => Path.Combine(RutaBase, "JSON", "citas.json");

        public static string Especialidades => Path.Combine(RutaBase,"JSON", "especialidades.json");

        public static void AsegurarDirectorio(string rutaArchivo)
        {
            string directorio = Path.GetDirectoryName(rutaArchivo);
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);
        }
    }

    public class Doctor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public bool Disponible { get; set; } = true;
        public bool Activo { get; set; } = true;
    }



    public class Paciente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string FechaNacimiento { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string TipoSangre { get; set; } = string.Empty;
        public string Alergias { get; set; } = string.Empty;
        public string Enfermedades { get; set; } = string.Empty;
        public string FechaRegistro { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }

    public class PacientesData
    {
        public List<Paciente> Pacientes { get; set; } = new();
    }

    public class DoctoresData
    {
        public List<Doctor> Doctores { get; set; } = new();
    }

    public class Cita
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int DoctorId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string DoctorNombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
    }
    public class CitasData
    {
        public List<Cita> Citas { get; set; } = new();
    
    

    }

}
