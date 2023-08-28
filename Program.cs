using System.IO.Ports;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

ushort vid = 0x041E;
ushort pid = 0x3260;

string aqs = SerialDevice.GetDeviceSelectorFromUsbVidPid(vid, pid);

var myDevices = Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs, null).GetResults();


if (myDevices.Count == 0)
{
    Console.WriteLine("Could not find any serial ports for the Katana v2");
    return;
}

SerialDevice device = Task.Run(() => SerialDevice.FromIdAsync(myDevices[0].Id).AsTask()).Result;

DataWriter dw = new DataWriter(device.OutputStream);

try
{
    Console.WriteLine("Successfully connected to " + device.PortName + "!");

    Console.WriteLine("Press any key to send the command...");
    Console.ReadKey();
    try
    {
        SendDataToPort(dw, "5a 3a 20 2b 00 01 01 ff 00 00 ff ff 00 ff 00 ff ff 00 00 ff 00 80 ff ff 00 ff ff ff bf 00 8c ff ff ff ff"); // Sets the 7 segments to Red, Green, Blue, Orange, Purple, Ye

    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not send first command");
        Console.WriteLine(ex.Message);
        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
        return;
    }

    Console.WriteLine("\nClosing program...");
}
catch (Exception ex)
{
    Console.WriteLine($"General Error: {ex.Message}");
    Console.WriteLine("Press any key to close...");
}

static void SendDataToPort(DataWriter deviceWriter, string data)
{
    byte[] bytesToSend = StringToByteArray(data);
    deviceWriter.WriteBytes(bytesToSend);
    deviceWriter.StoreAsync();
    deviceWriter.FlushAsync();
}

static byte[] StringToByteArray(string hex)
{
    string[] hexValues = hex.Split(' ');
    byte[] bytes = new byte[hexValues.Length];

    for (int i = 0; i < hexValues.Length; i++)
    {
        bytes[i] = Convert.ToByte(hexValues[i], 16);
    }

    return bytes;
}

