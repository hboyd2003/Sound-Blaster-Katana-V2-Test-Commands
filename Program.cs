using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.ExceptionServices;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.Devices.Custom;

ushort vid = 0x041E;
ushort pid = 0x3278;

string aqs = SerialDevice.GetDeviceSelectorFromUsbVidPid(vid, pid);

var myDevices = Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs, null).GetResults();

if (myDevices.Count == 0)
{
    Console.WriteLine("Could not find any serial ports for the Katana v2");
    return;
}

SerialDevice device;
try
{
    device = Task.Run(() => SerialDevice.FromIdAsync(myDevices[0].Id).AsTask()).Result;
} catch (Exception e)
{
    Console.WriteLine("Could not get serial port from device!");
    Console.WriteLine($"{e.Message}");
    return;

}


try
{
    SerialPort port = new SerialPort(device.PortName);
    Console.WriteLine("Successfully found device at " + device.PortName + "!");
    Console.WriteLine("Unlocking Device!");
    var proc = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = @"cudsp600_firmware_utility.exe",
            Arguments = $"auto ver /dv{vid:X} /dp{pid:X}", // Just gets the version so that it will unlock the device for us but not mess anything up.
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        }
    };
    proc.Start();
    proc.WaitForExit();

    Console.WriteLine("Unlocked Device (hopefully)");

    device.Dispose();

    Console.WriteLine("Press any key to send the command...");
    Console.ReadKey();
    try
    {
        //SendDataToPort(dw, "77 68 6f 61 72 65 79 6f 75 1e 04 60 32 e8 a0 1a 17 5c 93 61 a0 bc 47 2c d5 bc c7 3c 1b 5c b3 e0 56 5c ab c4 b7 5c b3 c0 56 5c b3 e0 56 0d 0a");
        //SendDataToPort(dw, "77 68 6f 61 72 65 79 6f 75 2e 4d 79 41 70 70 38 0d 0a");
        Console.WriteLine("Connecting to device " + device.PortName + "!");
        port.Open();
        Console.WriteLine("Connected to device " + device.PortName + "!");
        port.ReadTimeout = 1000;
        port.WriteTimeout = 1000;

        byte[] command = new byte[] { 0x5a, 0x03, 0x00};


        command = new byte[]
        {
            0x53, 0x57, 0x5f, 0x4d, 0x4f, 0x44, 0x45, 0x31,
            0x0d, 0x0a
        };
        port.Write(command, 0, command.Length);

        // Turn on LEDs (if they are off)
        command = new byte[] { 0x5a, 0x3a, 0x02, 0x25, 0x01 };
        port.Write(command, 0, command.Length);
        command = new byte[] { 0x5a, 0x3a, 0x02, 0x26, 0x01 };
        port.Write(command, 0, command.Length);
        command = new byte[] {
            0x5a, 0x3a, 0x20, 0x2b, 0x00, 0x01, 0x01, 0xff, 0x00, 0x00, 0xff, 0xff, 0x00, 0xff, 0x00, 0xff, 0xff, 0x00, 0x00, 0xff, 0x00, 0x80, 0xff, 0xff, 0x00, 0xff, 0xff, 0xff, 0xbf, 0x00, 0x8c, 0xff, 0xff, 0xff, 0xff
        }; // Sets the 7 segments to Red, Green, Blue, Orange, Purple, Yellow, White

        port.Write(command, 0, command.Length);
        port.Close();
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



