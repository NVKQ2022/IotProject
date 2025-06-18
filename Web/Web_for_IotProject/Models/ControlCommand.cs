namespace Web_for_IotProject.Models
{
    public class ControlCommand
    {
        public string DeviceId { get; set; }
        public string Command { get; set; }
        public object Parameter { get; set; }
    }
}
