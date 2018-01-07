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

            PeUtility.SubSystemType subsysVal;
            var subsysOffset = peFile.MainHeaderOffset;
            var headerType = peFile.Is32BitHeader ? typeof(IMAGE_OPTIONAL_HEADER32) : typeof(IMAGE_OPTIONAL_HEADER64);
            subsysVal = (PeUtility.SubSystemType)peFile.OptionalHeader64.Subsystem;
            subsysOffset += Marshal.OffsetOf(headerType, "Subsystem").ToInt32();

            switch (subsysVal)
            {
                case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
                    Console.WriteLine("Executable file is already a Win32 App!");
                    break;
                case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
                    Console.WriteLine("Console app detected.\r\nConverting...");

                    var subsysSetting = BitConverter.GetBytes((UInt16)PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI);

                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(subsysSetting);

                    if (peFile.Stream.CanWrite)
                    {
                        peFile.Stream.Seek(subsysOffset, SeekOrigin.Begin);
                        peFile.Stream.Write(subsysSetting, 0, subsysSetting.Length);
                    }
                    else
                    {
                        Console.WriteLine("Can't write changes! Check priviledges is sufficient or not.");
                    }

                    Console.WriteLine("Conversion Complete...");
                    break;
                default:
                    Console.WriteLine($"Unsupported subsystem : {Enum.GetName(typeof(PeUtility.SubSystemType), subsysVal)}.");
                    return;
            }
            peFile.Dispose();
        }

        static void Main(string[] args)
        {
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
