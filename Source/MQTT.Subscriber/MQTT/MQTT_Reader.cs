using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using MQTTnet.Client.Subscribing;
using MQTT.Shared.DTO;

namespace MQTT.Subscriber.MQTT
{
    internal class MQTT_Reader
    {
        public List<Telemetry> data { get; set; } = new List<Telemetry>();

        public delegate void NewDataReceivedEventHandler(object sender, Telemetry telemetry);
        public event NewDataReceivedEventHandler DataReceived;

        private readonly IMqttClient mqttClient;
        private readonly IMqttClientOptions options;
        private readonly string topic;

        public MQTT_Reader(string ip, int port, MqttProtocolVersion mqttProtocolVersion, string topic)
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();
            this.topic = topic;

            options = new MqttClientOptionsBuilder()
                .WithTcpServer(ip, port)
                .WithProtocolVersion(mqttProtocolVersion)
                .Build();

            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("✅ Conectado a MQTT Broker.");

                await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                Console.WriteLine("📡 Suscrito al tópico.");
            });

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var json = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                try
                {
                    var telemetry = JsonSerializer.Deserialize<Telemetry>(json) ?? new Telemetry();
                    DataReceived?.Invoke(this, telemetry);

                    lock (data)
                    {
                        data.Add(telemetry);
                        if (data.Count > 30)
                            data.RemoveAt(0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error parseando mensaje: {ex.Message}");
                }
            });
        }

        public async Task StartAsync()
        {
            try
            {
                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al conectar: {ex.Message}");
            }
        }
    }
}
