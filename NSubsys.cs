﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static NSubsys.PeUtility;

namespace NSubsys.Tasks
{
    public class NSubsys : Task
    {
        [Required]
        public string TargetFile { get; set; }

        public override bool Execute()
        {
            var fileInfo = new FileInfo(TargetFile);

            if (!fileInfo.Exists)
            {
                Log.LogError($"File doesn't exist! Path: '{TargetFile}'");
            }

            if (fileInfo.Extension.Equals("exe", StringComparison.OrdinalIgnoreCase))
            {
                Log.LogError("This tool only supports PE .exe files.");
            }

            if (Log.HasLoggedErrors)
            {
                return false;
            }

            return ProcessFile(fileInfo.FullName);
        }

        private bool ProcessFile(string exeFilePath)
        {
            Log.LogMessage("NSubsys : Subsystem Changer for Windows PE files.");

            using (var peFile = new PeUtility(exeFilePath))
            {
                PeUtility.SubSystemType subsysVal;
                var subsysOffset = peFile.MainHeaderOffset;
                var headerType = peFile.Is32BitHeader ? typeof(IMAGE_OPTIONAL_HEADER32) : typeof(IMAGE_OPTIONAL_HEADER64);

                if (peFile.Is32BitHeader)
                    subsysVal = (PeUtility.SubSystemType)peFile.OptionalHeader32.Subsystem;
                else
                    subsysVal = (PeUtility.SubSystemType)peFile.OptionalHeader64.Subsystem;


                subsysOffset += Marshal.OffsetOf(headerType, "Subsystem").ToInt32();

                switch (subsysVal)
                {
                    case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
                        Log.LogMessage("Executable file is already a Win32 App!");
                        return true;
                    case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
                        Log.LogMessage("Console app detected...");
                        Log.LogMessage("Converting...");

                        var subsysSetting = BitConverter.GetBytes((UInt16)PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI);

                        if (!BitConverter.IsLittleEndian)
                            Array.Reverse(subsysSetting);

                        if (peFile.Stream.CanWrite)
                        {
                            peFile.Stream.Seek(subsysOffset, SeekOrigin.Begin);
                            peFile.Stream.Write(subsysSetting, 0, subsysSetting.Length);
                            Log.LogMessage("Conversion Complete...");
                        }
                        else
                        {
                            Log.LogMessage("Can't write changes!");
                            Log.LogMessage("Conversion Failed...");
                        }

                        return true;
                    default:
                        Log.LogMessage($"Unsupported subsystem : {Enum.GetName(typeof(PeUtility.SubSystemType), subsysVal)}.");
                        return false;
                }
            }
        }
    }
}