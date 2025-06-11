using MQTT.Subscriber;
using MQTT.Subscriber.MQTT;
using MQTTnet.Formatter;

class Program
{
    static async Task Main(string[] args)
    {
        var reader = new MQTT_Reader(
            "localhost", 
            1884, 
            MqttProtocolVersion.V311, 
            "v1/devices/me/telemetry"
        );

        var influxWriter = new InfluxDBWriter(
            influxUrl: "http://localhost:8086",
            token: "my-super-token",
            bucket: "my-bucket",
            org: "Antares"
        );

        reader.DataReceived += async (sender, telemetry) =>
        {
            Console.WriteLine($"📡 Recibido de {telemetry.device_id}. Enviando a Influx...");
            await influxWriter.WriteTelemetryAsync(telemetry);
        };

        await reader.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }
}
