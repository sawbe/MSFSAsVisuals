Add this to MSFSs SimConnect.xml file (located in LocalAppData dir):

```
<SimConnect.Comm>
    <Descr>MSFSAsVisuals</Descr>
    <Protocol>Pipe</Protocol>
    <Scope>local</Scope>
    <Port>6969</Port>
    <MaxClients>64</MaxClients>
    <MaxRecvSize>41088</MaxRecvSize>
</SimConnect.Comm>
```

Server is configured to connect to P3D. Client connects to MSFS. 

To run on separate PCs start Client with argument of the server PC local IP Address eg. "Client.exe 192.168.0.100" otherwise 127.0.0.1 is used

Start client and server after both simulators are running

`sawbe-genericairliner` is a blank aircraft to select in MSFS (so there's no VC in the way) based on asobo 748. Put it in Community packages