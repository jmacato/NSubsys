using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using static C2GTool.PeUtility;

namespace C2GTool
{
    class Program
    {

        static void ProcessFile(string exeFilePath)
        {
            var peFile = new PeUtility(exeFilePath);

            PeUtility.SubSystemType SubsystemVal;
            var settingOffset = peFile.MainHeaderOffset;
            var headerType = peFile.Is32BitHeader ? typeof(IMAGE_OPTIONAL_HEADER32) : typeof(IMAGE_OPTIONAL_HEADER64);
            SubsystemVal = (PeUtility.SubSystemType)peFile.OptionalHeader64.Subsystem;
            settingOffset += Marshal.OffsetOf(headerType, "Subsystem").ToInt32();

            switch (SubsystemVal)
            {
                case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
                    Console.WriteLine("Executable file is already a Win32 App!");
                    break;
                case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
                    Console.WriteLine("Console app detected.\r\nConverting...");

                    var o = BitConverter.GetBytes((UInt16)PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI);

                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(o);

                    if (peFile.Stream.CanWrite)
                    {
                        peFile.Stream.Seek(settingOffset, SeekOrigin.Begin);
                        peFile.Stream.Write(o, 0, o.Length);
                    }
                    else
                    {
                        Console.WriteLine("Can't write changes! Check priviledges is sufficient or not.");
                    }

                    Console.WriteLine("Conversion Complete...");
                    break;
                default:
                    Console.WriteLine($"Unsupported subsystem : {Enum.GetName(typeof(PeUtility.SubSystemType), SubsystemVal)}.");
                    return;
            }
            peFile.Dispose();
        }

        static void Main(string[] args)
        {

#if DEBUG
            args = new string[] { @"C:\Users\Jumar\Desktop\kzanx\KzAn.exe" };
#endif

            if (args.Length > 0)
            {
                var inEXE = new FileInfo(args[0]);

                if (inEXE.Exists)
                {
                    if (inEXE.Extension.Equals("exe", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("This tool only supports PE .exe files.");
                    }
                    else
                    {
                        ProcessFile(inEXE.FullName);
                    }
                }
            }
            else
            {
                Console.WriteLine("No arguments given.");
            }

            Console.WriteLine("Exiting...");


        }
    }



}
