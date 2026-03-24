namespace COService.Infrastructure.Messaging;

/// <summary>
/// Options de configuration pour RabbitMQ
/// </summary>
public class RabbitMQOptions
{
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Active ou désactive RabbitMQ. Si false, le service de consommation ne démarrera pas.
    /// </summary>
    public bool Enabled { get; set; } = false;

    public string HostName { get; set; } = "192.168.2.119";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "sysguot";
    public string Password { get; set; } = "MyS3cur3Passwor_d";
    public string VirtualHost { get; set; } = "seg-co";
    public string Exchange { get; set; } = "evenements.co";
}
