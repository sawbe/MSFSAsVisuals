using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSFSAsVisuals.Shared;
using Newtonsoft.Json;
using XPlaneConnector;

namespace MSFSAsVisuals.Server
{
    public class XPSimConnector
    {
        private readonly XPlaneConnector.XPlaneConnector connector;
        private static TimeSpan queueInterval = TimeSpan.FromMilliseconds(15);//roughly 60fps
        private DateTime lastQueued = DateTime.MinValue;
        private SimObjectData data;
        private BlockingCollection<string> serverSendQueue;

        public XPSimConnector(BlockingCollection<string> sendQueue)
        {
            serverSendQueue = sendQueue;
            connector = new XPlaneConnector.XPlaneConnector();
            data = new SimObjectData();
            Subscribe();
            connector.Start();
        }

        private void Subscribe()
        {
            DataRefElement lat = new DataRefElement()
            {
                DataRef = "sim/flightmodel/position/latitude",
            };
            DataRefElement lon = new DataRefElement()
            {
                DataRef = "sim/flightmodel/position/longitude",
            };
            DataRefElement alt = new DataRefElement()
            {
                DataRef = "sim/flightmodel/position/elevation",
            };
            DataRefElement theta = new DataRefElement()
            {
                DataRef = "sim/flightmodel/position/theta",
                Description = "Pitch"
            };
            DataRefElement phi = new DataRefElement()
            {
                DataRef = "sim/flightmodel/position/phi",
                Description = "Roll"
            };
            DataRefElement psi = new DataRefElement()
            {
                DataRef = "sim/flightmodel/position/psi",
                Description = "True Heading"
            };

            connector.Subscribe(lat, 60, Process);
            connector.Subscribe(lon, 60, Process);
            connector.Subscribe(alt, 60, Process);
            connector.Subscribe(theta, 60, Process);
            connector.Subscribe(phi, 60, Process);
            connector.Subscribe(psi, 60, Process);
        }

        private void Process(DataRefElement element, float value)
        {
            string dataref = element.DataRef.Split('/').Last();
            switch(dataref)
            {
                case "latitude":
                    data.Latitude = ConvertToRadians(value);
                    break;
                case "longitude":
                    data.Longitude = ConvertToRadians(value);
                    break;
                case "elevation":
                    data.Altitude = ConvertToFeet(value);
                    break;
                case "theta":
                    data.Pitch = -value;
                    break;
                case "phi":
                    data.Bank = -value;
                    break;
                case "psi":
                    data.Heading = value;
                    break;
            }

            if(DateTime.UtcNow - lastQueued > queueInterval)
            {
                string json = "P:" + JsonConvert.SerializeObject(data);
                serverSendQueue.Add(json);
                lastQueued = DateTime.UtcNow;
                Console.WriteLine(json);
            }
        }
        private float ConvertToRadians(float angle)
        {
            return (float)(Math.PI / 180 * angle);
        }
        private float ConvertToFeet(float meters)
        {
            return meters * 3.28084f;
        }
    }
}
