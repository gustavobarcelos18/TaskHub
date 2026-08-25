namespace MinhaPrimeiraAPI.Services;

public sealed class EtiquetaDuplicadaException : Exception
{
    public EtiquetaDuplicadaException() : base("Já existe uma etiqueta com este nome.") { }
}
