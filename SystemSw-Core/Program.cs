using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SystemSw_Core
{
    class Program
    {

        static SerialPort serialPort;
        static CancellationTokenSource cts;
        static SemaphoreSlim ss = new SemaphoreSlim(0);

        static void Main(string[] args)
        {
            cts = new CancellationTokenSource();
            serialPort = new SerialPort
            {
                PortName = "COM5",
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                DtrEnable = true,
                RtsEnable = false,
                ReadTimeout = 1000,
                //WriteTimeout = 1000,
                Encoding = System.Text.Encoding.ASCII
            };
            serialPort.Open();
            serialPort.ProperlyInitializeNonsense(0x1A, 0xFF, 0xFF, 0x11, 0x13, 0, 768);
            if (!serialPort.IsOpen) return;

            Console.CancelKeyPress += (o, e) =>
            {
                //cts.Cancel();
                cts.Cancel();
            };

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();

            // Kick off read thread
            Task.Run(Read, cts.Token);

            //Console.WriteLine("> I");
            ////serialPort.Write(new byte[] { 0x49 }, 0, 1);
            //serialPort.ReadExisting();
            //serialPort.Write("q");

            while(!cts.Token.IsCancellationRequested)
            {
                Console.Write("> ");
                var command = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(command)) continue;
                serialPort.Write(command);
                ss.Release();
            }

            //var buffer = new byte[1];
            //var response = serialPort.Read(buffer, 0, 1);
            //Console.WriteLine($"response: {response}");
            //Console.ReadLine();
            serialPort.Close();


        }

        static void Read()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    ss.Wait();
                    string message = serialPort.ReadLine();
                    Console.WriteLine();
                    Console.WriteLine($"< {message}");
                    Console.WriteLine();
                    Console.Write("> ");
                }
                catch (TimeoutException) { }
            }
        }


    }


    internal static class SerialPortExtensions
    {
        public static void ProperlyInitializeNonsense(
            this SerialPort port, byte eof, byte error, byte @break,
            byte xon, byte xoff, ushort xOnLimit, ushort xOffLimit)
        {
            if (port == null)
                throw new NullReferenceException();
            if (port.BaseStream == null)
                throw new InvalidOperationException("Cannot change X chars until after the port has been opened.");

            try
            {
                // Get the base stream and its type which is System.IO.Ports.SerialStream
                object baseStream = port.BaseStream;
                Type baseStreamType = baseStream.GetType();

                // Get the Win32 file handle for the port
                SafeFileHandle portFileHandle = (SafeFileHandle)baseStreamType.GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseStream);

                // Get the value of the private DCB field (a value type)
                FieldInfo dcbFieldInfo = baseStreamType.GetField("_dcb", BindingFlags.NonPublic | BindingFlags.Instance);
                object dcbValue = dcbFieldInfo.GetValue(baseStream);

                // The type of dcb is Microsoft.Win32.UnsafeNativeMethods.DCB which is an internal type. We can only access it through reflection.
                Type dcbType = dcbValue.GetType();
                dcbType.GetField("XonChar").SetValue(dcbValue, xon);
                dcbType.GetField("XoffChar").SetValue(dcbValue, xoff);
                dcbType.GetField("EofChar").SetValue(dcbValue, eof);
                dcbType.GetField("ErrorChar").SetValue(dcbValue, error);
                dcbType.GetField("XonLim").SetValue(dcbValue, xOnLimit);
                dcbType.GetField("XoffLim").SetValue(dcbValue, xOffLimit);

                //dcbType.GetField("XoffChar").SetValue(dcbValue, xoff);

                // We need to call SetCommState but because dcbValue is a private type, we don't have enough
                //  information to create a p/Invoke declaration for it. We have to do the marshalling manually.

                // Create unmanaged memory to copy DCB into
                IntPtr hGlobal = Marshal.AllocHGlobal(Marshal.SizeOf(dcbValue));
                try
                {
                    // Copy their DCB value to unmanaged memory
                    Marshal.StructureToPtr(dcbValue, hGlobal, false);

                    // Call SetCommState
                    if (!SetCommState(portFileHandle, hGlobal))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Update the BaseStream.dcb field if SetCommState succeeded
                    dcbFieldInfo.SetValue(baseStream, dcbValue);
                }
                finally
                {
                    if (hGlobal != IntPtr.Zero)
                        Marshal.FreeHGlobal(hGlobal);
                }
            }
            catch (SecurityException) { throw; }
            catch (OutOfMemoryException) { throw; }
            catch (Win32Exception) { throw; }
            catch (Exception ex)
            {
                throw new ApplicationException("SetXonXoffChars has failed due to incorrect assumptions about System.IO.Ports.SerialStream which is an internal type.", ex);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle hFile, IntPtr lpDCB);
    }


}
