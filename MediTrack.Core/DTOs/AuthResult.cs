using MediTrack.Core.Models;

namespace MediTrack.Core.DTOs;

public class AuthResult
{
    public bool EsValido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public User? Usuario { get; set; }
}
