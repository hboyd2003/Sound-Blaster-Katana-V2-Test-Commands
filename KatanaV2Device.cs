using System.Reflection.Metadata;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using ABI.Windows.UI.Input.Inking.Analysis;
using System.Diagnostics;
using Windows.Networking;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using Windows.Devices.Usb;
using System;

namespace Sound_Blaster_Katana_V2_Test_Commands;
internal partial class KatanaV2Device
{

    public string DeviceName
    {
        get;
        private set;
    } = "Katana V2";

    public bool DeviceConnected
    {
        get;
        private set;
    }

    // ReSharper disable once InconsistentNaming
    public string UUID
    {
        get;
        private set;
    }

    public bool DeviceFound
    {
        get;
        private set;
    }


 
    private const ushort Vid = 0x1B1C;
    private const ushort Pid = 0x0C0B;

    private Regex serialNumberRegex = new (@"(?<=\d{4}\\)[\w\d]+");
    private SerialDevice? _device;
    private DataWriter? _deviceWriter;
    private int deleteme = 0;
    private static readonly string DeviceSelector = SerialDevice.GetDeviceSelectorFromUsbVidPid(Vid, Pid);
    private DeviceWatcher _deviceWatcher;
    public string? DeviceId
    {
        get;
        private set;
    }

    public KatanaV2Device()
    {
        Console.WriteLine("Starting Katanav2 class");
        _deviceWatcher = DeviceInformation.CreateWatcher(DeviceSelector);
        _deviceWatcher.Added += DeviceAddedEvent;
        _deviceWatcher.Removed += DeviceRemovedEvent;
        _deviceWatcher.Start();
        Console.WriteLine("Started search for KatanaV2");
    }

    // TODO: Proper async for KatanaV2's SendCommand
    public async Task<bool> SendCommand(byte[] command)
    {
        if (_deviceWriter == null)
        {
            return false;
        } 

        //TODO: Check if command sent successfully
        if (deleteme < 5)
        {
            deleteme++;
            Console.WriteLine("Sending command");
        }
        _deviceWriter.WriteBytes(command);
        try
        {
            _deviceWriter.StoreAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to send command");
            return false;
        }
        if (deleteme < 5)
        {
            deleteme++;
            Console.WriteLine("Sent command");
        }
        return true;
    }

    // TODO: Proper async for KatanaV2's DeviceAddedEvent
    private async void DeviceAddedEvent(DeviceWatcher sender, DeviceInformation deviceInfo)
    {
        Console.WriteLine("Katana V2 Found!");
        // We don't care what devices already exist until we want to connect
        if (DeviceConnected || DeviceFound)
        {
            Console.WriteLine("New device found while there is already a device connected/found");
            return;
        }
        DeviceFound = true;
        DeviceId = deviceInfo.Id;
        DeviceName = deviceInfo.Name;

        UUID = serialNumberRegex.Match(deviceInfo.Id).Groups[0].Value;

    }

    // TODO: Proper async for KatanaV2's DeviceRemovedEvent
    private async void DeviceRemovedEvent(DeviceWatcher sender, DeviceInformationUpdate deviceInfo)
    {
        if (deviceInfo.Id != DeviceId)
        {
            return;
        }

        if (DeviceFound)
        {
            DeviceId = null;
            DeviceFound = false;
        }

        if (DeviceConnected)
        {
            DisconnectFromDevice();
        }
    }

    // TODO: Proper async for KatanaV2's ErrorReceivedEvent
    private async void ErrorReceivedEvent(SerialDevice sender, ErrorReceivedEventArgs eventArgs)
    { 
        DisconnectFromDevice();
    }

    public async Task<bool> UnlockDevice()
    {
        if (!DeviceFound || DeviceConnected || DeviceId == null)
        {
            Console.WriteLine("Device unlock was called when it should not have been.");
            return false;
        }


        var firmwareUtilityProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "cudsp600_firmware_utility.exe"),
                Arguments = $"auto ver /dv{Vid:X} /dp{Pid:X}", // Just gets the version so that it will unlock the device for us but not mess anything up.
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        try
        {
            firmwareUtilityProcess.Start();
        }
        catch (Win32Exception ex)
        {
            if (ex.NativeErrorCode == 2) // File not found error code.
            {
                Console.WriteLine("Could not find cudsp600_firmware_utility.exe");
            }
            Console.WriteLine("Failed to run cudsp600_firmware_utility.exe");
            return false;
        }

        // Check if unlock was successful
        using var output = firmwareUtilityProcess.StandardOutput;
        {
            await firmwareUtilityProcess.WaitForExitAsync();
            var processOutput = await output.ReadToEndAsync();
            if (processOutput.Contains("unlock_comms [0]")) // Due to the programs poor logging there may be other random stuff before/after
            {
                return true;
            }
            Console.WriteLine("Failed to unlock device:\n\nOutput of cudsp600_firmware_utility.exe:\n" + processOutput);
            return false;

        }
        
    }

    // TODO: Proper async for KatanaV2's ConnectToDevice
    public async Task<bool> ConnectToDevice()
    {
        if (DeviceId == null || !DeviceFound || DeviceConnected)
        {
            return false;
        }

        if (!await UnlockDevice())
        {
            return false;
        }
        _device = await SerialDevice.FromIdAsync(DeviceId);
        Console.WriteLine("Got serial port object");
        

        _deviceWriter = new DataWriter(_device.OutputStream);
        Console.WriteLine("Opened serial port");

        //TODO: Check if device was actually connected.
        DeviceConnected = true;

        Console.WriteLine("Turning on LEDs");

        // Turn on LEDs (if they are off)
        await SendCommand(new byte[] { 0x5a, 0x3a, 0x02, 0x25, 0x01 });
        SendCommand(new byte[] { 0x5a, 0x3a, 0x02, 0x26, 0x01 });
        Console.WriteLine("Finished Connecting to device.");

        //var errorReceivedEventHandler = new Windows.Foundation.TypedEventHandler<SerialDevice, ErrorReceivedEventArgs>(this.ErrorReceivedEvent);
        //_device.ErrorReceived += errorReceivedEventHandler;

        return true;
    }

    // TODO: Proper async for KatanaV2's DisconnectFromDevice
    public async Task<bool> DisconnectFromDevice()
    {
        try
        {
            if (DeviceConnected)
            {
                _device?.Dispose();
                DeviceConnected = false;
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }


}
