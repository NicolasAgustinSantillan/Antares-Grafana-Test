using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MQTT.Subscriber.MQTT;

namespace MQTT.Subscriber
{
    public class InfluxDBWriter : IDisposable
    {
        private readonly InfluxDBClient influxClient;
        private readonly string bucket;
        private readonly string org;
        private readonly WriteApiAsync writeApi;

        public InfluxDBWriter(string influxUrl, string token, string bucket, string org)
        {
            this.bucket = bucket;
            this.org = org;
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(influxUrl)
                .AuthenticateToken(token.ToCharArray())
                .Build();

            this.influxClient = new InfluxDBClient(options);

            this.writeApi = influxClient.GetWriteApiAsync();
        }

        public async Task WriteTelemetryAsync(Telemetry telemetry)
        {
            var point = PointData
                .Measurement("telemetry")
                .Tag("device_id", telemetry.device_id)
                .Field("temperature", telemetry.temperature)
                .Field("humidity", telemetry.humidity)
                .Field("pressure", telemetry.pressure)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            await writeApi.WritePointAsync(point, bucket, org);
        }

        public void Dispose()
        {
            influxClient.Dispose();
        }
    }
}
