using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

namespace C2GTool
{
    class Program
    {
        static void Main(string[] args)
        {

#if DEBUG
            args = new string[] { @"C:\Users\Jumar\Desktop\kzanx\KzAn.exe" };
#endif

            if (args.Length > 0)
            {
                var inEXE = args[0];
                var peFile = new PeUtility(inEXE);
                PeUtility.SubSystemType SubsystemVal;
                IntPtr settingOffset;

                if (peFile.Is32BitHeader)
                {
                    SubsystemVal = (PeUtility.SubSystemType)peFile.OptionalHeader32.Subsystem;
                    settingOffset = Marshal.OffsetOf(typeof(PeUtility.IMAGE_OPTIONAL_HEADER32), "Subsystem");
                }
                else
                {
                    SubsystemVal = (PeUtility.SubSystemType)peFile.OptionalHeader64.Subsystem;
                    settingOffset = Marshal.OffsetOf(typeof(PeUtility.IMAGE_OPTIONAL_HEADER64), "Subsystem");

                }

                switch (SubsystemVal)
                {
                    case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
                        Console.WriteLine("Executable file is already a Win32 App!");

                        break;
                    case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
                        Console.WriteLine("Console app detected.\r\nConverting...");


                        break;
                    default:
                        Console.WriteLine($"Unsupported subsystem : {Enum.GetName(typeof(PeUtility.SubSystemType), SubsystemVal)}.");
                        return;
                }

                peFile.Dispose();

            }



        }
    }



}
