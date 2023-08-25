using System.IO.Ports;

using (SerialPort comPort = new SerialPort("COM3", 9600)) // Baudrate is unknown
{
    try
    {
        try {
            comPort.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not open COM3");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
            comPort.Close();
        }
        Console.WriteLine("Successfully connected to COM3!");

        Console.WriteLine("Press any key to send the first command...");
        Console.ReadKey();
        try
        {
            SendDataToPort(comPort, "5a 3a 08 2b 00 01 01 ff 00 80 00"); // Should set all leds to #008000
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
        
        Console.WriteLine("\nDid the RGB LEDs change? possible answers: (y/n)");
        char response = Console.ReadKey().KeyChar;
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

