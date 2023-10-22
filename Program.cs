using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Sound_Blaster_Katana_V2_Test_Commands;

KatanaV2Device katanaV2Device = new KatanaV2Device();
for (int i = 0; katanaV2Device.DeviceFound == false && i <=5; i++)
{
    Thread.Sleep(1000);
}

if (katanaV2Device.DeviceFound == false)
{
    Console.WriteLine("Device not found after five seconds");
    return;
}

katanaV2Device.ConnectToDevice();

Console.WriteLine("Device ID:" + katanaV2Device.DeviceId);
Console.WriteLine("Device Name: " + katanaV2Device.DeviceName);
Console.WriteLine("Device UUID: " + katanaV2Device.UUID);
