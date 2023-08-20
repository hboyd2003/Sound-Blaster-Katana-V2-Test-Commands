using System.IO.Ports;

using (SerialPort comPort = new SerialPort("COM3", 9600)) // Baudrate is unknown
{
    try
    {
        comPort.Open();
        Console.WriteLine("Successfully connected to COM3!");

        Console.WriteLine("Press any key to send the first command...");
        Console.ReadKey();

        SendDataToPort(comPort, "5a 3a 08 2b 00 01 01 ff 00 80 00"); // Should set all leds to #008000

        Console.WriteLine("\nDid the RGB LEDs change? (y/n)");
        char response = Console.ReadKey().KeyChar;

        if (response == 'n' || response == 'N')
        {
            SendDataToPort(comPort, "08 2b 00 01 01 ff 00 80 00"); // Serial communication was captured using a general USB capture program (serial header?)
            Console.WriteLine("\nSecond command sent. Check the LEDs again.");
        }
        else
        {
            Console.WriteLine("\nClosing program...");
        }

        comPort.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
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

