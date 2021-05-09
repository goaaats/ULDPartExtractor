using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Lumina;
using Lumina.Data.Files;

namespace ULDPartExtractor
{
    class UldExtractor
    {
        private readonly GameData data;

        public UldExtractor(GameData data)
        {
            this.data = data;
        }

        public void HandleUld(string path)
        {
            var outPath = Path.Combine("out", Path.GetFileNameWithoutExtension(path));
            Directory.CreateDirectory(outPath);

            var uld = this.data.GetFile<UldFile>(path);

            var loadedTex = new Dictionary<uint, Image>();
            var loadedTexHr = new Dictionary<uint, Image>();

            foreach (var textureEntry in uld.AssetData)
            {
                var texPath = new string(textureEntry.Path).Replace("\0", string.Empty);

                if (string.IsNullOrEmpty(texPath))
                    continue;

                loadedTex.Add(textureEntry.Id, this.GetImage(texPath));
                loadedTexHr.Add(textureEntry.Id, this.GetImage(texPath.Replace(".tex", "_hr1.tex")));
            }

            foreach (var partsData in uld.Parts)
            {
                for (var i = 0; i < partsData.PartCount; i++)
                {
                    var part = partsData.Parts[i];

                    var tex = loadedTex.GetValueOrDefault(part.TextureId);
                    var texHr = loadedTexHr.GetValueOrDefault(part.TextureId);

                    if (tex == null || texHr == null)
                    {
                        Console.WriteLine($"Could not find texture for part {partsData.Id}[{i}] in {path}({part.TextureId})");
                        continue;
                    }

                    var retImg = CloneRect(tex, new Rectangle(part.U, part.V, part.W, part.H));
                    retImg.Save(Path.Combine(outPath, $"{Path.GetFileName(path)}-{partsData.Id}-{i}.png"), ImageFormat.Png);

                    var retImgHr = CloneRect(texHr, new Rectangle(part.U * 2, part.V * 2, part.W * 2, part.H * 2));
                    retImgHr.Save(Path.Combine(outPath, $"{Path.GetFileName(path)}-{partsData.Id}-{i}-hr.png"), ImageFormat.Png);
                }
            }
        }

        private static Image CloneRect(Image image, Rectangle rect)
        {
            var srcBitmap = image as Bitmap;

            var clone = srcBitmap.Clone(rect, srcBitmap.PixelFormat);

            return clone;
        }

        private Image GetImage(string path) => GetImage(this.data.GetFile<TexFile>(path));

        private static unsafe Image GetImage(TexFile tex)
        {
            // this is terrible please find something better or get rid of .net imaging altogether
            Image image;
            fixed (byte* p = tex.ImageData)
            {
                var ptr = (IntPtr)p;
                using var tempImage = new Bitmap(tex.Header.Width, tex.Header.Height, tex.Header.Width * 4, PixelFormat.Format32bppArgb, ptr);
                image = new Bitmap(tempImage);
            }

            return image;
        }
    }
}
