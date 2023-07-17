using JeremyAnsel.DirectX.D3D11;
using JeremyAnsel.DirectX.Dxgi;
using JeremyAnsel.DirectX.DXMath;
using JeremyAnsel.DirectX.GameWindow;
using JeremyAnsel.Xwa.Opt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaHullIconGenerator
{
    class OptComponent : IGameComponent
    {
        private DeviceResources deviceResources;

        private readonly Dictionary<string, D3D11ShaderResourceView> textureViews = new Dictionary<string, D3D11ShaderResourceView>();

        private readonly List<OptMesh> meshes = new List<OptMesh>();

        private D3D11VertexShader vertexShader;

        private D3D11InputLayout inputLayout;

        private D3D11PixelShader pixelShader;

        private D3D11Buffer constantBuffer;

        private ConstantBufferData constantBufferData;

        private D3D11SamplerState sampler;

        private D3D11RasterizerState rasterizerState;

        private D3D11DepthStencilState depthStencilState0;

        private D3D11DepthStencilState depthStencilState1;

        private D3D11BlendState blendState0;

        private D3D11BlendState blendState1;

        public OptComponent(string optFilename, int version, string optObjectProfile, List<string> optObjectSkins)
            : this(XMMatrix.Identity, XMMatrix.Identity, optFilename, version, optObjectProfile, optObjectSkins)
        {
        }

        public OptComponent(XMFloat4X4 world, XMFloat4X4 view, string optFilename, int version, string optObjectProfile, List<string> optObjectSkins)
        {
            this.WorldMatrix = world;
            this.ViewMatrix = view;
            this.OptFileName = optFilename;
            this.OptVersion = version;
            this.OptObjectProfile = optObjectProfile ?? "Default";
            this.OptObjectSkins.AddRange(optObjectSkins ?? new());
        }

        public D3D11FeatureLevel MinimalFeatureLevel => D3D11FeatureLevel.FeatureLevel91;

        public string OptFileName { get; private set; }

        public int OptVersion { get; private set; }

        public string OptObjectProfile { get; private set; }

        public List<string> OptObjectSkins { get; } = new();

        public float OptSize { get; private set; }

        public Vector OptSpanSize { get; private set; }

        public Vector OptCenter { get; private set; }

        public XMFloat4X4 WorldMatrix
        {
            get { return ((XMMatrix)this.constantBufferData.World).Transpose(); }
            set { this.constantBufferData.World = ((XMMatrix)value).Transpose(); }
        }

        public XMFloat4X4 ViewMatrix
        {
            get { return ((XMMatrix)this.constantBufferData.View).Transpose(); }
            set { this.constantBufferData.View = ((XMMatrix)value).Transpose(); }
        }

        public XMFloat4X4 ProjectionMatrix
        {
            get { return ((XMMatrix)this.constantBufferData.Projection).Transpose(); }
            set { this.constantBufferData.Projection = ((XMMatrix)value).Transpose(); }
        }

        private static string _currentOptKey;
        private static OptFile _currentOpt;

        private static OptFile GetOptFile(string optFilename, int version, string optObjectProfile, List<string> optObjectSkins)
        {
            string key = string.Join(";", optFilename, version, optObjectProfile, string.Join(",", optObjectSkins));

            if (key == _currentOptKey)
            {
                return _currentOpt;
            }

            OptFile opt = OptFile.FromFile(optFilename);

            var objectProfiles = OptModel.GetObjectProfiles(optFilename);
            //var objectSkins = OptModel.GetSkins(optFilename);

            if (!objectProfiles.TryGetValue(optObjectProfile, out List<int> objectProfile))
            {
                objectProfile = objectProfiles["Default"];
            }

            opt = OptModel.GetTransformedOpt(opt, version, objectProfile, optObjectSkins);

            _currentOpt = opt;
            _currentOptKey = key;

            return _currentOpt;
        }

        public void CreateDeviceDependentResources(DeviceResources resources)
        {
            this.deviceResources = resources;

            //string fileName = Path.GetDirectoryName(this.OptFileName) + "\\" + Path.GetFileNameWithoutExtension(this.OptFileName) + "Exterior.opt";

            //OptFile opt;
            //if (File.Exists(fileName))
            //{
            //    opt = OptFile.FromFile(fileName);
            //}
            //else
            //{
            //    opt = OptFile.FromFile(this.OptFileName);
            //}

            OptFile opt = GetOptFile(this.OptFileName, this.OptVersion, this.OptObjectProfile, this.OptObjectSkins);

            this.OptSize = opt.Size * OptFile.ScaleFactor;
            this.OptSpanSize = opt.SpanSize.Scale(OptFile.ScaleFactor, OptFile.ScaleFactor, OptFile.ScaleFactor);

            Vector max = opt.MaxSize;
            Vector min = opt.MinSize;

            this.OptCenter = new Vector()
            {
                X = (max.X + min.X) / 2,
                Y = (max.Y + min.Y) / 2,
                Z = (max.Z + min.Z) / 2
            }.Scale(OptFile.ScaleFactor, OptFile.ScaleFactor, OptFile.ScaleFactor);

            this.CreateTextures(opt);
            this.CreateMeshes(opt);

            byte[] vertexShaderBytecode = File.ReadAllBytes("VertexShader.cso");
            this.vertexShader = this.deviceResources.D3DDevice.CreateVertexShader(vertexShaderBytecode, null);

            D3D11InputElementDesc[] basicVertexLayoutDesc = new D3D11InputElementDesc[]
            {
                new D3D11InputElementDesc
                {
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    Format = DxgiFormat.R32G32B32Float,
                    InputSlot = 0,
                    AlignedByteOffset = 0,
                    InputSlotClass = D3D11InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new D3D11InputElementDesc
                {
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    Format = DxgiFormat.R32G32B32Float,
                    InputSlot = 0,
                    AlignedByteOffset = 12,
                    InputSlotClass = D3D11InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new D3D11InputElementDesc
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Format = DxgiFormat.R32G32Float,
                    InputSlot = 0,
                    AlignedByteOffset = 24,
                    InputSlotClass = D3D11InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                }
            };

            this.inputLayout = this.deviceResources.D3DDevice.CreateInputLayout(basicVertexLayoutDesc, vertexShaderBytecode);

            byte[] pixelShaderBytecode = File.ReadAllBytes("PixelShader.cso");
            this.pixelShader = this.deviceResources.D3DDevice.CreatePixelShader(pixelShaderBytecode, null);

            var constantBufferDesc = new D3D11BufferDesc(ConstantBufferData.Size, D3D11BindOptions.ConstantBuffer);
            this.constantBuffer = this.deviceResources.D3DDevice.CreateBuffer(constantBufferDesc);

            D3D11SamplerDesc samplerDesc = new D3D11SamplerDesc(
                D3D11Filter.Anisotropic,
                D3D11TextureAddressMode.Wrap,
                D3D11TextureAddressMode.Wrap,
                D3D11TextureAddressMode.Wrap,
                0.0f,
                this.deviceResources.D3DFeatureLevel > D3D11FeatureLevel.FeatureLevel91 ? D3D11Constants.DefaultMaxAnisotropy : D3D11Constants.FeatureLevel91DefaultMaxAnisotropy,
                D3D11ComparisonFunction.Never,
                new float[] { 0.0f, 0.0f, 0.0f, 0.0f },
                0.0f,
                float.MaxValue);

            this.sampler = this.deviceResources.D3DDevice.CreateSamplerState(samplerDesc);

            D3D11RasterizerDesc rasterizerDesc = D3D11RasterizerDesc.Default;
            rasterizerDesc.CullMode = D3D11CullMode.None;
            this.rasterizerState = this.deviceResources.D3DDevice.CreateRasterizerState(rasterizerDesc);

            this.depthStencilState0 = this.deviceResources.D3DDevice.CreateDepthStencilState(D3D11DepthStencilDesc.Default);

            D3D11DepthStencilDesc depthStencilDesc = D3D11DepthStencilDesc.Default;
            depthStencilDesc.DepthWriteMask = D3D11DepthWriteMask.Zero;
            this.depthStencilState1 = this.deviceResources.D3DDevice.CreateDepthStencilState(depthStencilDesc);

            this.blendState0 = this.deviceResources.D3DDevice.CreateBlendState(D3D11BlendDesc.Default);

            D3D11BlendDesc blendDesc = D3D11BlendDesc.Default;
            D3D11RenderTargetBlendDesc[] blendDescRenderTargets = blendDesc.GetRenderTargets();
            blendDescRenderTargets[0].IsBlendEnabled = true;
            blendDescRenderTargets[0].SourceBlend = D3D11BlendValue.SourceAlpha;
            blendDescRenderTargets[0].DestinationBlend = D3D11BlendValue.InverseSourceAlpha;
            blendDescRenderTargets[0].BlendOperation = D3D11BlendOperation.Add;
            blendDescRenderTargets[0].SourceBlendAlpha = D3D11BlendValue.One;
            blendDescRenderTargets[0].DestinationBlendAlpha = D3D11BlendValue.InverseSourceAlpha;
            blendDescRenderTargets[0].BlendOperationAlpha = D3D11BlendOperation.Add;
            blendDescRenderTargets[0].RenderTargetWriteMask = D3D11ColorWriteEnables.All;
            blendDesc.SetRenderTargets(blendDescRenderTargets);
            this.blendState1 = this.deviceResources.D3DDevice.CreateBlendState(blendDesc);
        }

        private void CreateTextures(OptFile opt)
        {
            foreach (var textureKey in opt.Textures)
            {
                var optTextureName = textureKey.Key;
                var optTexture = textureKey.Value;

                int mipLevels = optTexture.MipmapsCount;

                if (mipLevels == 0)
                {
                    continue;
                }

                D3D11SubResourceData[] textureSubResData = new D3D11SubResourceData[mipLevels];

                int bpp = optTexture.BitsPerPixel;

                if (bpp == 8)
                {
                    List<XMUInt3> palette = Enumerable.Range(0, 256)
                        .Select(i =>
                        {
                            ushort c = BitConverter.ToUInt16(optTexture.Palette, 8 * 512 + i * 2);

                            byte r = (byte)((c & 0xF800) >> 11);
                            byte g = (byte)((c & 0x7E0) >> 5);
                            byte b = (byte)(c & 0x1F);

                            r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                            g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                            b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                            return new XMUInt3(r, g, b);
                        })
                        .ToList();

                    for (int level = 0; level < mipLevels; level++)
                    {
                        byte[] imageData = optTexture.GetMipmapImageData(level, out int width, out int height);
                        byte[] alphaData = optTexture.GetMipmapAlphaData(level, out int w, out int h);

                        int size = width * height;
                        byte[] textureData = new byte[size * 4];

                        for (int i = 0; i < size; i++)
                        {
                            int colorIndex = imageData[i];
                            XMUInt3 color = palette[colorIndex];

                            textureData[i * 4 + 0] = (byte)color.Z;
                            textureData[i * 4 + 1] = (byte)color.Y;
                            textureData[i * 4 + 2] = (byte)color.X;
                            textureData[i * 4 + 3] = alphaData != null ? alphaData[i] : (byte)255;
                        }

                        textureSubResData[level] = new D3D11SubResourceData(textureData, (uint)width * 4);
                    }
                }
                else if (bpp == 32)
                {
                    for (int level = 0; level < mipLevels; level++)
                    {
                        byte[] imageData = optTexture.GetMipmapImageData(level, out int width, out int height);

                        textureSubResData[level] = new D3D11SubResourceData(imageData, (uint)width * 4);
                    }
                }
                else
                {
                    continue;
                }

                D3D11Texture2DDesc textureDesc = new D3D11Texture2DDesc(DxgiFormat.B8G8R8A8UNorm, (uint)optTexture.Width, (uint)optTexture.Height, 1, (uint)textureSubResData.Length);

                using (var texture = this.deviceResources.D3DDevice.CreateTexture2D(textureDesc, textureSubResData))
                {
                    D3D11ShaderResourceViewDesc textureViewDesc = new D3D11ShaderResourceViewDesc
                    {
                        Format = textureDesc.Format,
                        ViewDimension = D3D11SrvDimension.Texture2D,
                        Texture2D = new D3D11Texture2DSrv
                        {
                            MipLevels = textureDesc.MipLevels,
                            MostDetailedMip = 0
                        }
                    };

                    this.textureViews.Add(optTextureName, this.deviceResources.D3DDevice.CreateShaderResourceView(texture, textureViewDesc));
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0017:Simplifier l'initialisation des objets", Justification = "Justified!")]
        private void CreateMeshes(OptFile opt)
        {
            var meshes3D = new List<Mesh3D>();

            foreach (var optMesh in opt.Meshes)
            {
                var positions = optMesh
                    .Vertices
                    .Select(t => new XMFloat3(-t.X * OptFile.ScaleFactor, t.Z * OptFile.ScaleFactor, -t.Y * OptFile.ScaleFactor))
                    .ToList();

                var normals = optMesh
                    .VertexNormals
                    .Select(t => new XMFloat3(t.X, t.Z, t.Y))
                    .ToList();

                var textureCoordinates = optMesh
                    .TextureCoordinates
                    .Select(t => new XMFloat2(t.U, -t.V))
                    .ToList();

                var optLod = optMesh.Lods.FirstOrDefault();

                if (optLod == null)
                {
                    continue;
                }

                foreach (var optFaceGroup in optLod.FaceGroups)
                {
                    var mesh3D = new Mesh3D();
                    meshes3D.Add(mesh3D);

                    var optTextureName = optFaceGroup.Textures[0];

                    mesh3D.Texture = optTextureName;

                    opt.Textures.TryGetValue(optTextureName, out Texture texture);
                    mesh3D.HasAlpha = texture == null ? false : texture.HasAlpha;

                    ushort index = 0;

                    foreach (var optFace in optFaceGroup.Faces)
                    {
                        Indices positionsIndex = optFace.VerticesIndex;
                        Indices normalsIndex = optFace.VertexNormalsIndex;
                        Indices textureCoordinatesIndex = optFace.TextureCoordinatesIndex;

                        BasicVertex vertex = new BasicVertex();

                        vertex.Position = positions.ElementAtOrDefault(positionsIndex.C);
                        vertex.Normal = normals.ElementAtOrDefault(normalsIndex.C);
                        vertex.TextureCoordinates = textureCoordinates.ElementAtOrDefault(textureCoordinatesIndex.C);
                        mesh3D.Vertices.Add(vertex);
                        mesh3D.Indices.Add(index);
                        index++;

                        vertex.Position = positions.ElementAtOrDefault(positionsIndex.B);
                        vertex.Normal = normals.ElementAtOrDefault(normalsIndex.B);
                        vertex.TextureCoordinates = textureCoordinates.ElementAtOrDefault(textureCoordinatesIndex.B);
                        mesh3D.Vertices.Add(vertex);
                        mesh3D.Indices.Add(index);
                        index++;

                        vertex.Position = positions.ElementAtOrDefault(positionsIndex.A);
                        vertex.Normal = normals.ElementAtOrDefault(normalsIndex.A);
                        vertex.TextureCoordinates = textureCoordinates.ElementAtOrDefault(textureCoordinatesIndex.A);
                        mesh3D.Vertices.Add(vertex);
                        mesh3D.Indices.Add(index);
                        index++;

                        if (positionsIndex.D >= 0)
                        {
                            mesh3D.Indices.Add((ushort)(index - 3U));
                            mesh3D.Indices.Add((ushort)(index - 1U));

                            vertex.Position = positions.ElementAtOrDefault(positionsIndex.D);
                            vertex.Normal = normals.ElementAtOrDefault(normalsIndex.D);
                            vertex.TextureCoordinates = textureCoordinates.ElementAtOrDefault(textureCoordinatesIndex.D);
                            mesh3D.Vertices.Add(vertex);
                            mesh3D.Indices.Add(index);
                            index++;
                        }
                    }
                }
            }

            foreach (var mesh3d in meshes3D)
            {
                var mesh = new OptMesh(this.deviceResources, mesh3d);
                this.meshes.Add(mesh);
            }
        }

        public void ReleaseDeviceDependentResources()
        {
            foreach (var textureKey in this.textureViews)
            {
                textureKey.Value.Dispose();
            }

            this.textureViews.Clear();

            foreach (var mesh in this.meshes)
            {
                mesh.ReleaseDeviceDependentResources();
            }

            this.meshes.Clear();

            D3D11Utils.DisposeAndNull(ref this.vertexShader);
            D3D11Utils.DisposeAndNull(ref this.inputLayout);
            D3D11Utils.DisposeAndNull(ref this.pixelShader);
            D3D11Utils.DisposeAndNull(ref this.constantBuffer);
            D3D11Utils.DisposeAndNull(ref this.sampler);
            D3D11Utils.DisposeAndNull(ref this.rasterizerState);
            D3D11Utils.DisposeAndNull(ref this.depthStencilState0);
            D3D11Utils.DisposeAndNull(ref this.depthStencilState1);
            D3D11Utils.DisposeAndNull(ref this.blendState0);
            D3D11Utils.DisposeAndNull(ref this.blendState1);
        }

        public void CreateWindowSizeDependentResources()
        {
            var viewport = this.deviceResources.ScreenViewport;

            this.ProjectionMatrix = XMMatrix.PerspectiveFovRH(XMMath.ConvertToRadians(70.0f), viewport.Width / viewport.Height, 0.01f, 100.0f);
        }

        public void ReleaseWindowSizeDependentResources()
        {
        }

        public void Update(ITimer timer)
        {
        }

        public void Render()
        {
            var context = this.deviceResources.D3DContext;

            context.UpdateSubresource(this.constantBuffer, 0, null, this.constantBufferData, 0, 0);

            context.InputAssemblerSetInputLayout(this.inputLayout);

            context.InputAssemblerSetPrimitiveTopology(D3D11PrimitiveTopology.TriangleList);

            context.VertexShaderSetShader(this.vertexShader, null);
            context.VertexShaderSetConstantBuffers(0, new[] { this.constantBuffer });

            context.PixelShaderSetShader(this.pixelShader, null);
            context.PixelShaderSetSamplers(0, new[] { this.sampler });

            //context.RasterizerStageSetState(this.rasterizerState);

            context.OutputMergerSetDepthStencilState(this.depthStencilState0, 0);
            context.OutputMergerSetBlendState(this.blendState0, null, 0xffffffff);

            foreach (var mesh in this.meshes.Where(t => !t.HasAlpha))
            {
                context.InputAssemblerSetVertexBuffers(
                    0,
                    new[] { mesh.VertexBuffer },
                    new uint[] { BasicVertex.Size },
                    new uint[] { 0 });

                context.InputAssemblerSetIndexBuffer(mesh.IndexBuffer, DxgiFormat.R16UInt, 0);

                this.textureViews.TryGetValue(mesh.Texture, out D3D11ShaderResourceView texture);

                context.PixelShaderSetShaderResources(0, new[] { texture });

                context.DrawIndexed(mesh.IndicesCount, 0, 0);
            }

            context.OutputMergerSetDepthStencilState(this.depthStencilState1, 0);
            context.OutputMergerSetBlendState(this.blendState1, null, 0xffffffff);

            foreach (var mesh in this.meshes.Where(t => t.HasAlpha))
            {
                context.InputAssemblerSetVertexBuffers(
                    0,
                    new[] { mesh.VertexBuffer },
                    new uint[] { BasicVertex.Size },
                    new uint[] { 0 });

                context.InputAssemblerSetIndexBuffer(mesh.IndexBuffer, DxgiFormat.R16UInt, 0);

                this.textureViews.TryGetValue(mesh.Texture, out D3D11ShaderResourceView texture);

                context.PixelShaderSetShaderResources(0, new[] { texture });

                context.DrawIndexed(mesh.IndicesCount, 0, 0);
            }
        }
    }
}
