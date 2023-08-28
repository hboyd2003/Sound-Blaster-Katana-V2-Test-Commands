using System.IO.Ports;
using Windows.Devices.SerialCommunication;

ushort vid = 0x041E;
ushort pid = 0x3260;

string aqs = SerialDevice.GetDeviceSelectorFromUsbVidPid(vid, pid);

var myDevices = Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs, null).GetResults();


if (myDevices.Count == 0)
{
    Console.WriteLine("Could not find any serial ports for the Katana v2");
    return;
}


using SerialDevice device = await SerialDevice.FromIdAsync(myDevices[0].Id).GetResults();
{
    using (SerialPort comPort = new SerialPort(device.PortName, 9600)) // Baudrate is unknown
    {
        try
        {
            try
            {
                comPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not open " + device.PortName);
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                comPort.Close();
            }
            Console.WriteLine("Successfully connected to " + device.PortName + "!");

            Console.WriteLine("Press any key to send the command...");
            Console.ReadKey();
            try
            {
                SendDataToPort(comPort, "5a 3a 20 2b 00 01 01 ff ff 00 00 ff 00 ff 00 ff 00 00 ff ff ff 80 00 ff ff ff 00 ff 8c 00 bf ff ff ff ff"); // Sets the 7 segments to Red, Green, Blue, Orange, Purple, Yellow, White
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not send first command");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                comPort.Close();
                return;
            }

            Console.WriteLine("\nClosing program...");

            comPort.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
            Console.WriteLine("Press any key to close...");
        }
    }

    static void SendDataToPort(SerialPort port, string data)
    {
        byte[] bytesToSend = StringToByteArray(data);
        port.Write(bytesToSend, 0, bytesToSend.Length);
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
}

