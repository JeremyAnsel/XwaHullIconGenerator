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
    struct BasicVertex
    {
        public static readonly uint Size = (uint)Marshal.SizeOf(typeof(BasicVertex));

        public XMFloat3 Position;

        public XMFloat3 Normal;

        public XMFloat2 TextureCoordinates;

        public BasicVertex(XMFloat3 position, XMFloat3 normal, XMFloat2 textureCoordinates)
        {
            this.Position = position;
            this.Normal = normal;
            this.TextureCoordinates = textureCoordinates;
        }
    }
}
