﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace MusicMirror.Synchronization
{
	internal static class SymbolicLinkNativeMethods
	{
		private const uint genericReadAccess = 0x80000000;

		private const uint fileFlagsForOpenReparsePointAndBackupSemantics = 0x02200000;

		private const int ioctlCommandGetReparsePoint = 0x000900A8;

		private const uint openExisting = 0x3;

		private const uint pathNotAReparsePointError = 0x80071126;

		private const uint shareModeAll = 0x7; // Read, Write, Delete

		private const uint symLinkTag = 0xA000000C;

		private const int targetIsAFile = 0;

		private const int targetIsADirectory = 1;

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern SafeFileHandle CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
		[return:MarshalAs(UnmanagedType.Bool)]
		static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return:MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeviceIoControl(
			IntPtr hDevice,
			uint dwIoControlCode,
			IntPtr lpInBuffer,
			int nInBufferSize,
			IntPtr lpOutBuffer,
			int nOutBufferSize,
			out int lpBytesReturned,
			IntPtr lpOverlapped);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static void CreateDirectoryLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, targetIsADirectory) || Marshal.GetLastWin32Error() != 0)
			{
				try
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
				catch (COMException exception)
				{
					throw new IOException(exception.Message, exception);
				}
			}
		}

		public static void CreateFileLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, targetIsAFile))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static bool Exists(string path)
		{
			if (!Directory.Exists(path) && !File.Exists(path))
			{
				return false;
			}
			string target = GetTarget(path);
			return target != null;
		}

		private static SafeFileHandle getFileHandle(string path)
		{
			return CreateFile(path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting,
				fileFlagsForOpenReparsePointAndBackupSemantics, IntPtr.Zero);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
		public static string GetTarget(string path)
		{
			SymbolicLinkReparseData reparseDataBuffer;

			using (SafeFileHandle fileHandle = getFileHandle(path))
			{
				if (fileHandle.IsInvalid)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}

				int outBufferSize = Marshal.SizeOf(typeof(SymbolicLinkReparseData));
				IntPtr outBuffer = IntPtr.Zero;
				try
				{
					outBuffer = Marshal.AllocHGlobal(outBufferSize);
					int bytesReturned;
					bool success = DeviceIoControl(
						fileHandle.DangerousGetHandle(), ioctlCommandGetReparsePoint, IntPtr.Zero, 0,
						outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);
										
					if (!success)
					{
						if (((uint)Marshal.GetHRForLastWin32Error()) == pathNotAReparsePointError)
						{
							return null;
						}
						Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
					}

					reparseDataBuffer = (SymbolicLinkReparseData)Marshal.PtrToStructure(
						outBuffer, typeof(SymbolicLinkReparseData));
				}
				finally
				{
					Marshal.FreeHGlobal(outBuffer);
				}
			}
			if (reparseDataBuffer.ReparseTag != symLinkTag)
			{
				return null;
			}

			string target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
				reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

			return target;
		}
	}

	/// <remarks>
	/// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct SymbolicLinkReparseData
	{
		// Not certain about this!
		private const int maxUnicodePathLength = 260 * 2;

		public uint ReparseTag;
		public ushort ReparseDataLength;
		public ushort Reserved;
		public ushort SubstituteNameOffset;
		public ushort SubstituteNameLength;
		public ushort PrintNameOffset;
		public ushort PrintNameLength;
		public uint Flags;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength)]
		public byte[] PathBuffer;
	}


}
