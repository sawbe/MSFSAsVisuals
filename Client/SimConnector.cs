using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;
using MSFSAsVisuals.Shared;

namespace MSFSAsVisuals.Client
{
    public class SimConnector
    {
        private const string moduleName = "MSFSAsVisuals.Client";
        private SimConnect simConnect;
        private CancellationTokenSource cancellationTokenSource;
        private Task simConnectPoll;

        public SimConnector(uint configIndex)
        {
            simConnect = new SimConnect(moduleName, IntPtr.Zero, 0, null, configIndex);
            simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;

            Config();
            Subscribe();
            FreezeUser(true);

            cancellationTokenSource = new CancellationTokenSource();
            simConnectPoll = new Task(SimConnectPoll, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            simConnectPoll.Start();
        }

        public void ProcessSimData(SimObjectDataRequest request, object data)
        {
            simConnect.SetDataOnSimObject(request, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, data);
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

        }

        private void Config()
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

            foreach (SimEvent simConnectEvent in Enum.GetValues(typeof(SimEvent)))
            {
                simConnect.MapClientEventToSimEvent(simConnectEvent, simConnectEvent.ToString());
                simConnect.AddClientEventToNotificationGroup(SimEventGroup.Default, simConnectEvent, false);
            }
        }

        private void Subscribe()
        {

        }

        private void FreezeUser(bool active)
        {
            uint b = (uint)(active ? 1 : 0);
            simConnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, SimEvent.FREEZE_LATITUDE_LONGITUDE_SET, b, SimEventGroup.Default, SIMCONNECT_EVENT_FLAG.DEFAULT);
            simConnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, SimEvent.FREEZE_ATTITUDE_SET, b, SimEventGroup.Default, SIMCONNECT_EVENT_FLAG.DEFAULT);
            simConnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, SimEvent.FREEZE_ALTITUDE_SET, b, SimEventGroup.Default, SIMCONNECT_EVENT_FLAG.DEFAULT);
        }
    }
}
