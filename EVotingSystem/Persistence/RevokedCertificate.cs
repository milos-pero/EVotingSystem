public class RevokedCertificate
{
    public string SerialNumber { get; set; } = "";
    public DateTime RevokedAt { get; set; }
    public string Reason { get; set; } = "";
}
