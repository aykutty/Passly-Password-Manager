namespace Passly.Options;

public class PasswordHashingOptions
{
    public int DegreeOfParallelism { get; set; }
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
    public int SaltSize { get; set; }
    public int HashSize { get; set; }
}