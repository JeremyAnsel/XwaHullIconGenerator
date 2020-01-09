using JeremyAnsel.DirectX.D3D11;
using JeremyAnsel.DirectX.DXMath;
using JeremyAnsel.DirectX.GameWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaHullIconGenerator
{
    static class HullIconGenerator
    {
        public static byte[] Generate(uint width, uint height, string optFileName, RenderFace side, RenderMethod method)
        {
            var options = new DeviceResourcesOptions
            {
                UseHighestFeatureLevel = false,
                PreferMultisampling = true
            };

            var deviceResources = new RenderTargetDeviceResources(width, height, D3D11FeatureLevel.FeatureLevel100, options);
            var optComponent = new OptComponent(optFileName);

            try
            {
                optComponent.CreateDeviceDependentResources(deviceResources);
                optComponent.CreateWindowSizeDependentResources();
                optComponent.Update(null);

                optComponent.WorldMatrix =
                    XMMatrix.Translation(optComponent.OptCenter.X, -optComponent.OptCenter.Z, optComponent.OptCenter.Y)
                    * XMMatrix.ScalingFromVector(XMVector.Replicate(0.95f / optComponent.OptSize));

                switch (side)
                {
                    case RenderFace.Top:
                    default:
                        optComponent.ViewMatrix = XMMatrix.LookAtRH(new XMFloat3(0, 2, 0), new XMFloat3(0, 0, 0), new XMFloat3(0, 0, 1));
                        break;

                    case RenderFace.Front:
                        optComponent.ViewMatrix = XMMatrix.LookAtRH(new XMFloat3(0, 0, 2), new XMFloat3(0, 0, 0), new XMFloat3(0, 1, 0));
                        break;

                    case RenderFace.Side:
                        optComponent.ViewMatrix = XMMatrix.LookAtRH(new XMFloat3(2, 0, 0), new XMFloat3(0, 0, 0), new XMFloat3(0, 1, 0));
                        break;
                }

                optComponent.ProjectionMatrix = XMMatrix.OrthographicRH(1, 1, 0, 5);

                var context = deviceResources.D3DContext;
                context.OutputMergerSetRenderTargets(new[] { deviceResources.D3DRenderTargetView }, deviceResources.D3DDepthStencilView);
                context.ClearRenderTargetView(deviceResources.D3DRenderTargetView, new[] { 0.0f, 0.0f, 0.0f, 0.0f });
                context.ClearDepthStencilView(deviceResources.D3DDepthStencilView, D3D11ClearOptions.Depth, 1.0f, 0);

                optComponent.Render();
                deviceResources.Present();

                var buffer = deviceResources.GetBackBufferContent();

                for (int i = 0; i < width * height; i++)
                {
                    byte a = buffer[i * 4 + 3];
                    buffer[i * 4 + 3] = a == 0 ? (byte)0 : (byte)255;
                }

                switch (method)
                {
                    case RenderMethod.Color:
                    default:
                        return buffer;

                    case RenderMethod.GrayScaleLight:
                        return ConvertToGrayScaleLight(buffer, (int)width, (int)height);

                    case RenderMethod.GrayScaleDark:
                        return ConvertToGrayScaleDark(buffer, (int)width, (int)height);

                    case RenderMethod.Blue:
                        return ConvertToBlue(buffer, (int)width, (int)height);
                }
            }
            finally
            {
                optComponent.ReleaseWindowSizeDependentResources();
                optComponent.ReleaseDeviceDependentResources();
                deviceResources.Release();
            }
        }

        private static byte[] ConvertToGrayScaleLight(byte[] buffer, int width, int height)
        {
            var buffer2 = new byte[width * height * 4];

            for (int i = 0; i < width * height; i++)
            {
                byte b = buffer[i * 4 + 0];
                byte g = buffer[i * 4 + 1];
                byte r = buffer[i * 4 + 2];
                byte a = buffer[i * 4 + 3];

                byte c = Math.Max(r, Math.Max(g, b));

                buffer2[i * 4 + 0] = c;
                buffer2[i * 4 + 1] = c;
                buffer2[i * 4 + 2] = c;
                buffer2[i * 4 + 3] = a;
            }

            return buffer2;
        }

        private static byte[] ConvertToGrayScaleDark(byte[] buffer, int width, int height)
        {
            var buffer2 = new byte[width * height * 4];

            for (int i = 0; i < width * height; i++)
            {
                byte b = buffer[i * 4 + 0];
                byte g = buffer[i * 4 + 1];
                byte r = buffer[i * 4 + 2];
                byte a = buffer[i * 4 + 3];

                byte c = (byte)((r + g + b) / 3);

                buffer2[i * 4 + 0] = c;
                buffer2[i * 4 + 1] = c;
                buffer2[i * 4 + 2] = c;
                buffer2[i * 4 + 3] = a;
            }

            return buffer2;
        }

        private static byte[] ConvertToBlue(byte[] buffer, int width, int height)
        {
            var buffer2 = new byte[width * height * 4];

            for (int i = 0; i < width * height; i++)
            {
                byte a = buffer[i * 4 + 3];

                byte c = a == 0 ? (byte)0 : (byte)255;

                buffer2[i * 4 + 0] = c;
                buffer2[i * 4 + 1] = 0;
                buffer2[i * 4 + 2] = 0;
                buffer2[i * 4 + 3] = a;
            }

            return buffer2;
        }
    }
}
