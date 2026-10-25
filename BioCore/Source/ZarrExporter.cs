using BioLib;
using ZarrNET.Core;
using ZarrNET.Core.OmeZarr.Metadata;
using PixelFormat = AForge.PixelFormat;

namespace BioCore
{
    // Exports fully loaded images. Slide readers expose only a viewport buffer,
    // so they must not be treated as complete planes here.
    internal static class ZarrExporter
    {
        internal static async Task SaveAsync(BioImage image, string destination)
        {
            if (File.Exists(destination) || Directory.Exists(destination))
                throw new IOException("The destination already exists. Choose a new folder name.");
            if (image.isPyramidal || image.IsZarrSource)
                throw new NotSupportedException("Saving pyramidal or Zarr-backed images is not yet supported. Save a fully loaded image instead.");
            if (image.Resolutions.Count == 0 || image.SizeZ <= 0 || image.SizeC <= 0 || image.SizeT <= 0)
                throw new InvalidOperationException("The image dimensions are incomplete.");

            var resolution = image.Resolutions[0];
            var format = resolution.PixelFormat;
            int bytesPerSample = format switch
            {
                PixelFormat.Format8bppIndexed or PixelFormat.Format24bppRgb => 1,
                PixelFormat.Format16bppGrayScale or PixelFormat.Format48bppRgb => 2,
                _ => throw new NotSupportedException($"The pixel format {format} is not supported for OME-Zarr export.")
            };
            bool rgb = format == PixelFormat.Format24bppRgb || format == PixelFormat.Format48bppRgb;
            if (rgb && image.SizeC != 1)
                throw new NotSupportedException("Interleaved RGB images with multiple logical channels are not supported.");
            int width = resolution.SizeX, height = resolution.SizeY;
            if (width <= 0 || height <= 0)
                throw new InvalidOperationException("The image has invalid dimensions.");
            int outputChannels = rgb ? 3 : image.SizeC;
            const int chunk = 512;
            var descriptor = new BioImageDescriptor(width, height, new ZarrNET.Core.ZCT(image.SizeZ, outputChannels, image.SizeT))
            {
                Name = Path.GetFileNameWithoutExtension(image.filename),
                DataType = bytesPerSample == 1 ? "uint8" : "uint16",
                PhysicalSizeX = image.PhysicalSizeX,
                PhysicalSizeY = image.PhysicalSizeY,
                PhysicalSizeZ = image.PhysicalSizeZ,
                ChunkX = chunk,
                ChunkY = chunk
            };

            string fullPath = Path.GetFullPath(destination);
            string temporary = fullPath + ".partial-" + Guid.NewGuid().ToString("N");
            try
            {
                var levels = new List<ResolutionLevelDescriptor> { new(width, height, 1.0) };
                await using (var writer = await OmeZarrWriter.CreateAsync(temporary, descriptor, levels).ConfigureAwait(false))
                {
                    for (int t = 0; t < image.SizeT; t++)
                    for (int z = 0; z < image.SizeZ; z++)
                    for (int c = 0; c < outputChannels; c++)
                    {
                        int index = image.GetFrameIndex(z, rgb ? 0 : c, t);
                        if (index < 0 || index >= image.Buffers.Count || image.Buffers[index] == null)
                            throw new InvalidOperationException($"Image plane Z{z} C{c} T{t} is missing.");
                        var buffer = image.Buffers[index];
                        if (buffer.SizeX != width || buffer.SizeY != height || buffer.PixelFormat != format || buffer.Bytes == null)
                            throw new InvalidOperationException("The image does not contain complete, matching pixel planes.");
                        int components = rgb ? 3 : 1;
                        int rowBytes = checked(width * components * bytesPerSample);
                        if (buffer.Stride < rowBytes || buffer.Bytes.Length < checked(buffer.Stride * height))
                            throw new InvalidOperationException("An image plane has an invalid pixel buffer.");
                        for (int y = 0; y < height; y += chunk)
                        for (int x = 0; x < width; x += chunk)
                        {
                            int w = Math.Min(chunk, width - x), h = Math.Min(chunk, height - y);
                            var tile = new byte[checked(w * h * bytesPerSample)];
                            for (int row = 0; row < h; row++)
                            for (int col = 0; col < w; col++)
                            {
                                // AForge's RGB buffers use BGR sample order.
                                int component = rgb ? 2 - c : 0;
                                int src = (y + row) * buffer.Stride + ((x + col) * components + component) * bytesPerSample;
                                int dst = (row * w + col) * bytesPerSample;
                                if (bytesPerSample == 2 && !buffer.LittleEndian)
                                {
                                    tile[dst] = buffer.Bytes[src + 1];
                                    tile[dst + 1] = buffer.Bytes[src];
                                }
                                else
                                    Buffer.BlockCopy(buffer.Bytes, src, tile, dst, bytesPerSample);
                            }
                            await writer.WriteRegionAsync(t, c, z, y, x, h, w, tile, levelIndex: 0).ConfigureAwait(false);
                        }
                    }
                }
                if (image.Annotations.Count > 0)
                    Zarr.SaveROIs(image, temporary);
                // Some Windows file-system filters briefly hold newly written
                // chunks after the writer closes. Retry a transient move failure.
                for (int attempt = 0; ; attempt++)
                {
                    try
                    {
                        Directory.Move(temporary, fullPath);
                        break;
                    }
                    catch (IOException) when (attempt < 3 && !File.Exists(fullPath) && !Directory.Exists(fullPath))
                    {
                        await Task.Delay(100 * (attempt + 1)).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (Directory.Exists(temporary))
                    Directory.Delete(temporary, true);
            }
        }
    }
}
