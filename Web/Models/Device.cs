using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Web_for_IotProject.Models
{
    public class Device
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string Location { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public bool Status { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public int Quality { get; set; }

        public int FPS { get; set; }
        public string LastSeen { get; set; }
    }

    
}
