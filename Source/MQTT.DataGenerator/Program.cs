using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", 1884)
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
            var telemetryData = new
            {
                device_id = "sensor-01",
                timestamp = DateTime.UtcNow,
                temperature = random.Next(20, 30),
                humidity = random.Next(40, 60),
                vibracion_rms = Math.Round(random.NextDouble() * 2, 2),
                pressure = random.Next(980, 1020),
                status = "ok"
            };

            var payload = JsonSerializer.Serialize(telemetryData);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("v1/devices/me/telemetry")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(message);
            Console.WriteLine($"📡 Telemetría enviada: {payload}");

            await Task.Delay(5000); // Espera 5 segundos antes de enviar el siguiente mensaje
        }
    }
}
