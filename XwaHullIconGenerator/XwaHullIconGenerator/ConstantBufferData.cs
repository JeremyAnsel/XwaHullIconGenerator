using JeremyAnsel.DirectX.DXMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XwaHullIconGenerator
{
    [StructLayout(LayoutKind.Sequential)]
    struct ConstantBufferData
    {
        public static readonly uint Size = (uint)Marshal.SizeOf(typeof(ConstantBufferData));

        public XMFloat4X4 World;

        public XMFloat4X4 View;

        public XMFloat4X4 Projection;
    }
}
