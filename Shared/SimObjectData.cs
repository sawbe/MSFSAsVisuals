using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFSAsVisuals.Shared
{
    public struct SimObjectData
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Pitch;
        public double Bank;
        public double Heading;
        public double OnGround;
    }
    public struct SimObjectAccelerationData
    {
        public double X;
        public double Y;
        public double Z;
        public double rX;
        public double rY;
        public double rZ;
    }
    public struct SimObjectVelocityData
    {
        public double X;
        public double Y;
        public double Z;
        public double rX;
        public double rY;
        public double rZ;
    }

    public enum SimObjectDataRequest
    {
        Plane,
        Accelerations,
        Velocities
    }
    public enum SimEventGroup
    {
        Default
    }
    public enum SimEvent
    {
        FREEZE_LATITUDE_LONGITUDE_SET,
        FREEZE_ALTITUDE_SET,
        FREEZE_ATTITUDE_SET,
    }
}
