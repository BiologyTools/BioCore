namespace BioCore
{
    internal static class ImageSeriesLoader
    {
        internal static async IAsyncEnumerable<BioImage> OpenAsync(string file)
        {
            int count = BioImage.GetSeriesCount(file);
            for (int series = 0; series < count;)
            {
                BioImage image = await BioImage.OpenFileAsync(file, series, false, true);
                if (image == null)
                    throw new InvalidOperationException("The image loader returned no image.");

                int seriesSpan = 1;
                // BioLib may collect consecutive series into one pyramid. Only skip
                // those native levels; nested and synthetic resolutions consume no series.
                if (image.Type == BioImage.ImageType.pyramidal &&
                    image.PyramidNativeLevelCount > 1 &&
                    image.imRead != null && image.imRead.getResolutionCount() == 1)
                {
                    seriesSpan = Math.Min(image.PyramidNativeLevelCount, count - series);
                }

                // Determine the next series before the viewer can change the reader's level.
                series += seriesSpan;
                yield return image;
            }
        }
    }
}
