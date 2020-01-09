using JeremyAnsel.DirectX.D3D11;
using JeremyAnsel.DirectX.GameWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaHullIconGenerator
{
    class OptMesh
    {
        private D3D11Buffer vertexBuffer;

        private D3D11Buffer indexBuffer;

        public OptMesh(DeviceResources deviceResources, Mesh3D mesh)
        {
            this.CreateDeviceDependentResources(deviceResources, mesh);
        }

        public D3D11Buffer VertexBuffer { get { return this.vertexBuffer; } }

        public D3D11Buffer IndexBuffer { get { return this.indexBuffer; } }

        public uint IndicesCount { get; private set; }

        public string Texture { get; private set; }

        public bool HasAlpha { get; private set; }

        public void CreateDeviceDependentResources(DeviceResources resources, Mesh3D mesh)
        {
            var vertices = mesh.Vertices.ToArray();
            var vertexBufferDesc = D3D11BufferDesc.From(vertices, D3D11BindOptions.VertexBuffer);
            this.vertexBuffer = resources.D3DDevice.CreateBuffer(vertexBufferDesc, vertices, 0, 0);

            var indices = mesh.Indices.ToArray();
            var indexBufferDesc = D3D11BufferDesc.From(indices, D3D11BindOptions.IndexBuffer);
            this.indexBuffer = resources.D3DDevice.CreateBuffer(indexBufferDesc, indices, 0, 0);
            this.IndicesCount = (uint)indices.Length;

            this.Texture = mesh.Texture;
            this.HasAlpha = mesh.HasAlpha;
        }

        public void ReleaseDeviceDependentResources()
        {
            D3D11Utils.DisposeAndNull(ref this.vertexBuffer);
            D3D11Utils.DisposeAndNull(ref this.indexBuffer);
        }
    }
}
