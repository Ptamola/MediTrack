namespace MediTrack.Core.Models;

/// <summary>
/// Perfil clinico ampliado del paciente. Complementa al usuario con datos sanitarios,
/// contacto de emergencia y ruta relativa de la foto.
/// </summary>
public class Patient
{
    public Guid IdUsuario { get; set; }
    public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-30);
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string ObservacionesGenerales { get; set; } = string.Empty;
    public string DniNie { get; set; } = string.Empty;
    public string Sexo { get; set; } = "No especificado";
    public string GrupoSanguineo { get; set; } = "No especificado";
    public decimal AlturaCm { get; set; }
    public decimal PesoKg { get; set; }
    public string Alergias { get; set; } = string.Empty;
    public string AntecedentesMedicos { get; set; } = string.Empty;
    public string ContactoEmergenciaNombre { get; set; } = string.Empty;
    public string ContactoEmergenciaTelefono { get; set; } = string.Empty;
    public string SeguroMedico { get; set; } = string.Empty;
    public string NumeroTarjetaSanitaria { get; set; } = string.Empty;
    public string FotoRuta { get; set; } = string.Empty;
}
