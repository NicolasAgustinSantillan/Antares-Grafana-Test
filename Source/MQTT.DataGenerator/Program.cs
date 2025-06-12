using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Text.Json;
using MQTT.Shared.DTO;
using System.Runtime.CompilerServices;

class Program
{
    static async Task Main(string[] args)
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        var serverAddress = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(serverAddress, 1884)
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .Build();

        try
        {
            await mqttClient.ConnectAsync(options);
            Console.WriteLine("✅ Conectado a ThingsBoard.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al conectar: {ex.Message}");
            return;
        }

        var random = new Random();

        while (true)
        {
            List<Telemetry> telemetrys = GetDevices().Select(divece => new Telemetry()
            {
                device_id = divece,
                timestamp = DateTime.UtcNow,
                temperature = random.Next(20, 30),
                humidity = random.Next(40, 60),
                vibracion_rms = Math.Round(random.NextDouble() * 2, 2),
                pressure = random.Next(980, 1020),
                status = "ok"
            }).ToList();

            foreach (var telemetry in telemetrys)
            {
                var payload = JsonSerializer.Serialize(telemetry);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("v1/devices/me/telemetry")
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message);
                Console.WriteLine($"📡 Telemetría enviada: {payload}");
                await Task.Delay(50); // Espera entre dispositivos
            }

            await Task.Delay(5000); // Espera 5 segundos antes de enviar el siguiente mensaje
        }
    }

    private static List<string> GetDevices()
    {
        string filePath = "telemetries.json";
        List<string>? deviceIds = new List<string>();

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            deviceIds = JsonSerializer.Deserialize<List<string>>(json);

            if (deviceIds != null)
            {
                Console.WriteLine("Lista de dispositivos configurados:");
            }
            else
            {
                throw new Exception("Archivo de dispositivos sin configurar :c");
            }
        }
        else
        {
            throw new Exception("No existe el archivo de configuracion :c");
        }

        return deviceIds;
    }
}
