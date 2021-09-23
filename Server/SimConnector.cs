using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LockheedMartin.Prepar3D.SimConnect;
using MSFSAsVisuals.Shared;
using Newtonsoft.Json;

namespace MSFSAsVisuals.Server
{
    public class SimConnector
    {
        private const string moduleName = "MSFSAsVisuals.Client";
        private SimConnect simConnect;
        private CancellationTokenSource cancellationTokenSource;
        private Task simConnectPoll;
        private BlockingCollection<string> serverSendQueue;

        public SimConnector(uint configIndex, BlockingCollection<string> sendQueue)
        {
            serverSendQueue = sendQueue;
            simConnect = new SimConnect(moduleName, IntPtr.Zero, 0, null, configIndex);
            simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;

            Subscribe();

            cancellationTokenSource = new CancellationTokenSource();
            simConnectPoll = new Task(SimConnectPoll, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            simConnectPoll.Start();
        }

        private void SimConnectPoll()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    simConnect.ReceiveMessage();
                }
                catch (COMException ex)
                {
                    cancellationTokenSource.Cancel();
                }
                if (!Thread.Yield())
                    Thread.Sleep(1);
            }
        }

        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            string json = "";
            switch((SimObjectDataRequest)data.dwRequestID)
            {
                case SimObjectDataRequest.Plane:
                    SimObjectData plane = (SimObjectData)data.dwData[0];
                    json = "P:"+JsonConvert.SerializeObject(plane);
                    //Console.WriteLine("Sent " + string.Join(", ", plane.Altitude, plane.Latitude, plane.Longitude, plane.Pitch, plane.Heading, plane.Bank, plane.OnGround));
                    break;
                case SimObjectDataRequest.Accelerations:
                    SimObjectAccelerationData accel = (SimObjectAccelerationData)data.dwData[0];
                    json = "A:"+JsonConvert.SerializeObject(accel);
                    //Console.WriteLine("Sent A:" + string.Join(", ", accel.X, accel.Y, accel.Z, accel.rX, accel.rY, accel.rZ));
                    break;
                case SimObjectDataRequest.Velocities:
                    SimObjectVelocityData vel = (SimObjectVelocityData)data.dwData[0];
                    json = "V:" + JsonConvert.SerializeObject(vel);
                    //Console.WriteLine("Sent V:" + string.Join(", ", vel.X, vel.Y, vel.Z, vel.rX, vel.rY, vel.rZ));
                    break;
            }
            serverSendQueue.Add(json);

            
        }

        private void Subscribe()
        {
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "PLANE LATITUDE", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "PLANE LONGITUDE", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 1);
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.01f, 2);
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.01f, 3);
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.01f, 4);
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.01f, 5);
            simConnect.AddToDataDefinition(SimObjectDataRequest.Plane, "SIM ON GROUND", "bool", SIMCONNECT_DATATYPE.FLOAT64, 1, 6);
            simConnect.RegisterDataDefineStruct<SimObjectData>(SimObjectDataRequest.Plane);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Accelerations, "ACCELERATION BODY X", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Accelerations, "ACCELERATION BODY Y", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Accelerations, "ACCELERATION BODY Z", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Accelerations, "ROTATION ACCELERATION BODY X", "radians per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Accelerations, "ROTATION ACCELERATION BODY Y", "radians per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Accelerations, "ROTATION ACCELERATION BODY Z", "radians per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.RegisterDataDefineStruct<SimObjectAccelerationData>(SimObjectDataRequest.Accelerations);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Velocities, "VELOCITY BODY X", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Velocities, "VELOCITY BODY Y", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Velocities, "VELOCITY BODY Z", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Velocities, "ROTATION VELOCITY BODY X", "radians per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Velocities, "ROTATION VELOCITY BODY Y", "radians per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.AddToDataDefinition(SimObjectDataRequest.Velocities, "ROTATION VELOCITY BODY Z", "radians per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0000001f, 0);
            //simConnect.RegisterDataDefineStruct<SimObjectVelocityData>(SimObjectDataRequest.Velocities);

            simConnect.RequestDataOnSimObject(SimObjectDataRequest.Plane, SimObjectDataRequest.Plane, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
            //simConnect.RequestDataOnSimObject(SimObjectDataRequest.Accelerations, SimObjectDataRequest.Accelerations, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
            //simConnect.RequestDataOnSimObject(SimObjectDataRequest.Velocities, SimObjectDataRequest.Velocities, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
        }
    }
}
