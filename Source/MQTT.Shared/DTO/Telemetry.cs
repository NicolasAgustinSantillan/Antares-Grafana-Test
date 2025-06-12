namespace MQTT.Shared.DTO
{
    public class Telemetry
    {
        public string? device_id { get; set; }
        public DateTime timestamp { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double vibracion_rms { get; set; }
        public double pressure { get; set; }
        public string? status { get; set; }
    }
}
