using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaHullIconGenerator
{
    class Mesh3D
    {
        public List<BasicVertex> Vertices { get; } = new List<BasicVertex>();

        public List<ushort> Indices { get; } = new List<ushort>();

        public string Texture { get; set; }

        public bool HasAlpha { get; set; }
    }
}
