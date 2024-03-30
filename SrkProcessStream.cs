using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK;

namespace BDxGraphiK
{
	public class SrkProcessStream
	{
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int BytesRead);

		[DllImport("kernel32.dll")]
		static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

		public Process BaseProcess;
		IntPtr hProcess;


		public SrkProcessStream(Process process)
		{
			OpenProcess(process);
		}

		public long BaseOffset = 0;

		public byte[] Read(long offset, int count)
		{
			int read;
			byte[] output = new byte[count];
			ReadProcessMemory(hProcess, BaseOffset + offset, output, count, out read);
			return output;
		}

		public void Read(long offset, ref byte[] bytes)
		{
			int read;
			ReadProcessMemory(hProcess, BaseOffset + offset, bytes, bytes.Length, out read);
		}

		public void Write(long offset, byte[] buffer, int count)
		{
			int written;
			var data = new byte[count];
			Array.Copy(buffer, 0, data, 0, buffer.Length < count ? buffer.Length : count);
			WriteProcessMemory(hProcess, BaseOffset + offset, data, count, out written);
		}

		public void WriteASCII(long offset, string text, int count)
		{
			byte[] buffer = System.Text.Encoding.ASCII.GetBytes(text);
			Write(offset, buffer, count);
		}

		public void WriteByte(long offset, byte val)
		{
			Write(offset, new byte[] { val }, 1);
		}

		public void WriteInt16(long offset, Int16 val)
		{
			Write(offset, BitConverter.GetBytes(val), 2);
		}

		public void WriteInt32(long offset, Int32 val)
		{
			Write(offset, BitConverter.GetBytes(val), 4);
		}
		public void WriteUInt32(long offset, UInt32 val)
		{
			Write(offset, BitConverter.GetBytes(val), 4);
		}

		public void WriteSingle(long offset, float val)
		{
			Write(offset, BitConverter.GetBytes(val), 4);
		}

		private void OpenProcess(Process process)
		{
			var permissions = 0x001FFFFF;
			hProcess = OpenProcess(permissions, true, process.Id);
			this.BaseProcess = process;
		}

		public static SrkProcessStream SearchProcess(string processName)
		{
			foreach (Process process in Process.GetProcesses())
			{
				if (process.ProcessName.Contains(processName))
				{
					return new SrkProcessStream(process);
				}
			}
			return null;
		}

		
		public String ReadString(long offset, int length, bool trimEnd)
		{
			byte[] buffer = Read(offset, length);

			if (trimEnd)
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] == 0)
					return System.Text.Encoding.ASCII.GetString(buffer, 0, i);
			}
			return System.Text.Encoding.ASCII.GetString(buffer);
		}

		public UInt32 ReadUInt32(long offset)
		{
			byte[] buffer = Read(offset, 4);

			return global::System.BitConverter.ToUInt32(buffer, 0);
		}

		public Int32 ReadInt32(long offset)
		{
			byte[] buffer = Read(offset, 4);

			return global::System.BitConverter.ToInt32(buffer, 0);
		}

		public Int64 ReadInt64(long offset)
		{
			byte[] buffer = Read(offset, 8);

			return global::System.BitConverter.ToInt64(buffer, 0);
		}
		public UInt64 ReadUInt64(long offset)
		{
			byte[] buffer = Read(offset, 8);

			return global::System.BitConverter.ToUInt64(buffer, 0);
		}

		public UInt16 ReadUInt16(long offset)
		{
			byte[] buffer = Read(offset, 2);

			return global::System.BitConverter.ToUInt16(buffer, 0);
		}

		public Int16 ReadInt16(long offset)
		{
			byte[] buffer = Read(offset, 2);

			return global::System.BitConverter.ToInt16(buffer, 0);
		}

		public SByte ReadSByte(long offset)
		{
			byte[] buffer = Read(offset, 1);

			return (SByte)buffer[0];
		}

		public Byte ReadByte(long offset)
		{
			byte[] buffer = Read(offset, 1);

			return buffer[0];
		}

		public Single ReadSingle(long offset)
		{
			byte[] buffer = Read(offset, 4);

			return global::System.BitConverter.ToSingle(buffer, 0);
		}

		public Matrix4 ReadMatrix4(long offset)
		{
			Matrix4 output = Matrix4.Identity;
			byte[] buffer = Read(offset, 0x40);


			output.M11 = global::System.BitConverter.ToSingle(buffer, 0x00);
			output.M12 = global::System.BitConverter.ToSingle(buffer, 0x04);
			output.M13 = global::System.BitConverter.ToSingle(buffer, 0x08);
			output.M14 = global::System.BitConverter.ToSingle(buffer, 0x0C);

			output.M21 = global::System.BitConverter.ToSingle(buffer, 0x10);
			output.M22 = global::System.BitConverter.ToSingle(buffer, 0x14);
			output.M23 = global::System.BitConverter.ToSingle(buffer, 0x18);
			output.M24 = global::System.BitConverter.ToSingle(buffer, 0x1C);

			output.M31 = global::System.BitConverter.ToSingle(buffer, 0x20);
			output.M32 = global::System.BitConverter.ToSingle(buffer, 0x24);
			output.M33 = global::System.BitConverter.ToSingle(buffer, 0x28);
			output.M34 = global::System.BitConverter.ToSingle(buffer, 0x2C);

			output.M41 = global::System.BitConverter.ToSingle(buffer, 0x30);
			output.M42 = global::System.BitConverter.ToSingle(buffer, 0x34);
			output.M43 = global::System.BitConverter.ToSingle(buffer, 0x38);
			output.M44 = global::System.BitConverter.ToSingle(buffer, 0x3C);

			return output;
		}

		public Matrix4[] ReadMatrices(long offset, int bufferLength, int offsetMatrices, int countMatrices, out byte[] buffer)
		{
			Matrix4[] output = new Matrix4[countMatrices];
			buffer = Read(offset, bufferLength);

			int readPosition = offsetMatrices;
			for (int i = 0; i < countMatrices; i++)
			{
				output[i] = Matrix4.Identity;
				output[i].M11 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x00);
				output[i].M12 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x04);
				output[i].M13 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x08);
				output[i].M14 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x0C);

				output[i].M21 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x10);
				output[i].M22 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x14);
				output[i].M23 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x18);
				output[i].M24 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x1C);

				output[i].M31 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x20);
				output[i].M32 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x24);
				output[i].M33 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x28);
				output[i].M34 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x2C);

				output[i].M41 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x30);
				output[i].M42 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x34);
				output[i].M43 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x38);
				output[i].M44 = global::System.BitConverter.ToSingle(buffer, readPosition + 0x3C);
				readPosition += 0x40;
			}

			return output;
		}



	}
}
