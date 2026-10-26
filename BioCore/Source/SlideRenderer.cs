using AForge;
using BioCore;
using BioLib;
using BruTile;
using OpenSlideGTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BioCore
{
    public class SlideRenderer
    {
        private readonly SlideGLArea _glArea;
        private OpenSlideBase _openSlideBase;
        private SlideBase _slideBase;
        private bool _useOpenSlide;
        private BioLib.TileCache _tileCache;
        private OpenSlideGTK.TileCache _openTileCache;
        private HashSet<TileIndex> _uploadedTiles = new();
        private int _currentLevel = -1;
        private int _renderVersion;
        private bool _hasContentKey;
        private int _contentZ;
        private int _contentC;
        private int _contentT;
        private BioImage? _contentRgbZarr;
        private int[]? _contentRgbSelection;

        public SlideRenderer(SlideGLArea glArea)
        {
            _glArea = glArea;
        }

        public void SetSource(OpenSlideBase source)
        {
            _openSlideBase = source;
            _slideBase = null;
            _useOpenSlide = true;
            _openTileCache = new OpenSlideGTK.TileCache(source, 200);
            ClearCache();
        }

        public void SetSource(SlideBase source)
        {
            _slideBase = source;
            _openSlideBase = null;
            _useOpenSlide = false;
            _tileCache = new BioLib.TileCache(source, 200);
            ClearCache();
        }

        public void ClearCache()
        {
            _renderVersion++;
            _glArea.ClearTextureCache();
            _uploadedTiles.Clear();
            _currentLevel = -1;
            _hasContentKey = false;
            _contentRgbZarr = null;
            _contentRgbSelection = null;
        }

        public async Task UpdateViewAsync(
            PointD pyramidalOrigin,
            int viewportWidth,
            int viewportHeight,
            double resolution,
            ZCT coordinate,
            BioImage? rgbZarr = null,
            int[]? rgbSelection = null)
        {
            if (_openSlideBase == null && _slideBase == null)
                return;
            if (viewportWidth <= 1 && viewportHeight <= 1)
                return;
            var schema = _useOpenSlide ? _openSlideBase.Schema : _slideBase.Schema;

            int level = TileUtil.GetLevel(schema.Resolutions, resolution);
            var levelRes = schema.Resolutions[level];
            double levelUnitsPerPixel = levelRes.UnitsPerPixel;

            int[]? requestedRgbSelection = rgbSelection?.ToArray();
            if (rgbZarr != null && requestedRgbSelection == null)
                requestedRgbSelection = new[] { 0, 1, 2 };
            bool contentChanged = !_hasContentKey
                || coordinate.Z != _contentZ
                || coordinate.C != _contentC
                || coordinate.T != _contentT
                || !ReferenceEquals(rgbZarr, _contentRgbZarr)
                || !SelectionsEqual(requestedRgbSelection, _contentRgbSelection);

            // Every invocation supersedes an older asynchronous viewport request.
            // A content or pyramid-level change also invalidates GPU textures because
            // their cache key is only TileIndex and does not include Z/C/T or RGB state.
            int renderVersion = ++_renderVersion;
            if (contentChanged || level != _currentLevel)
            {
                _glArea.ClearTextureCache();
                _uploadedTiles.Clear();
                _currentLevel = level;
            }

            _hasContentKey = true;
            _contentZ = coordinate.Z;
            _contentC = coordinate.C;
            _contentT = coordinate.T;
            _contentRgbZarr = rgbZarr;
            _contentRgbSelection = requestedRgbSelection;

            // Calculate world extent for the viewport
            double minX = pyramidalOrigin.X * resolution;
            double minY = -pyramidalOrigin.Y * resolution;
            double width = viewportWidth * resolution;
            double height = viewportHeight * resolution;
            var worldExtent = new Extent(minX, minY - height, minX + width, minY);

            // Get tiles that intersect the viewport
            var tileInfos = schema.GetTileInfos(worldExtent, level).ToList();

            if (tileInfos.Count == 0)
            {
                var pixelExtent = OpenSlideGTK.ExtentEx.WorldToPixelInvertedY(worldExtent, resolution);
                tileInfos = schema.GetTileInfos(pixelExtent, level).ToList();
            }

            int[]? rgbChannels = requestedRgbSelection;
            // OpenSlide tiles are fetched below through GetTileAsync. Its bulk
            // prefetch uses separate stitch state that this renderer never reads,
            // and fetching both ways can fail or duplicate the work.
            if (rgbZarr == null && !_useOpenSlide)
                await _slideBase.FetchTilesAsync(tileInfos.ToList(), level, coordinate, pyramidalOrigin, new AForge.Size(viewportWidth, viewportHeight));
            if (renderVersion != _renderVersion)
                return;

            var renderInfos = new List<TileRenderInfo>();
            foreach (var tileInfo in tileInfos)
            {
                if (!_glArea.HasTileTexture(tileInfo.Index))
                {
                    byte[] tileData;
                    if (rgbZarr != null)
                    {
                        int tileWidth = schema.GetTileWidth(level);
                        int tileHeight = schema.GetTileHeight(level);
                        int tilePixelWidth = (int)Math.Round(tileInfo.Extent.Width / levelUnitsPerPixel);
                        int tilePixelHeight = (int)Math.Round(tileInfo.Extent.Height / levelUnitsPerPixel);
                        tileData = await ZarrChannelView.ReadRgbTileAsync(rgbZarr, level,
                            tileInfo.Index.Col * tileWidth, tileInfo.Index.Row * tileHeight,
                            tilePixelWidth, tilePixelHeight, coordinate, rgbChannels!);
                    }
                    else if (_useOpenSlide)
                        tileData = await _openSlideBase.GetTileAsync(tileInfo);
                    else
                        tileData = await _slideBase.GetTileAsync(tileInfo, coordinate);
                    if (renderVersion != _renderVersion)
                        return;
                    if (tileData != null && rgbZarr == null && rgbChannels != null)
                    {
                        tileData = (byte[])tileData.Clone();
                        for (int i = 0; i + 3 < tileData.Length; i += 4)
                            for (int color = 0; color < 3; color++)
                                if (rgbChannels[color] < 0) tileData[i + color] = 0;
                    }
                    if (tileData != null)
                    {
                        int tW = (int)Math.Round(tileInfo.Extent.Width / levelUnitsPerPixel);
                        int tH = (int)Math.Round(tileInfo.Extent.Height / levelUnitsPerPixel);

                        // BioLib/OpenSlide return schema-sized buffers with black
                        // padding at image edges, while TileInfo exposes the clipped
                        // edge extent. Repack a padded edge tile before uploading it;
                        // otherwise OpenGL treats the clipped width as the source row
                        // stride and the lower rows come from unrelated buffer offsets.
                        if (rgbZarr == null)
                        {
                            int sourceWidth = schema.GetTileWidth(level);
                            int sourceHeight = schema.GetTileHeight(level);
                            if ((tW != sourceWidth || tH != sourceHeight) &&
                                tileData.Length >= sourceWidth * sourceHeight * 4)
                            {
                                byte[] clipped = new byte[tW * tH * 4];
                                int copyBytes = tW * 4;
                                for (int row = 0; row < tH; row++)
                                    Buffer.BlockCopy(tileData, row * sourceWidth * 4,
                                        clipped, row * copyBytes, copyBytes);
                                tileData = clipped;
                            }
                        }

                        _glArea.UploadTileTexture(tileInfo.Index, tileData, tW, tH);
                        _uploadedTiles.Add(tileInfo.Index);
                    }
                }

                if (_glArea.HasTileTexture(tileInfo.Index))
                {
                    var renderInfo = CalculateScreenPosition(
                        tileInfo,
                        pyramidalOrigin,
                        resolution,
                        level,
                        schema);
                    renderInfos.Add(renderInfo);
                }
            }

            _glArea.SetTilesToRender(renderInfos);
            _glArea.RequestRedraw();
        }

        private static bool SelectionsEqual(int[]? left, int[]? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left == null || right == null || left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; i++)
                if (left[i] != right[i])
                    return false;

            return true;
        }

        private async Task<byte[]> FetchTileAsync(TileInfo tileInfo, int level, ZCT coordinate)
        {
            try
            {
                if (_useOpenSlide)
                {
                    var bt = await _openTileCache.GetTile(new OpenSlideGTK.Info(coordinate, tileInfo.Index, tileInfo.Extent, level));
                    return bt;
                }
                else
                {
                    return await _tileCache.GetTile(new BioLib.TileInformation(tileInfo.Index, tileInfo.Extent, coordinate));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tile {tileInfo.Index}: {ex.Message}");
                return null;
            }
        }

        private TileRenderInfo CalculateScreenPosition(
            TileInfo tile,
            PointD pyramidalOrigin,
            double viewResolution,
            int level,
            ITileSchema schema)
        {
            // Get the actual tile size from schema (typically 256x256)
            int tileWidth = schema.GetTileWidth(level);
            int tileHeight = schema.GetTileHeight(level);

            // Convert tile extent to pixel coordinates
            var pixelExtent = OpenSlideGTK.ExtentEx.WorldToPixelInvertedY(tile.Extent, viewResolution);

            // Use approach 1: MinY with positive origin
            double tileLeftPx = pixelExtent.MinX;
            double tileTopPx = pixelExtent.MinY;
            double viewXPx = pyramidalOrigin.X;
            double viewYPx = pyramidalOrigin.Y;

            float screenX = (float)(tileLeftPx - viewXPx);
            float screenY = (float)(tileTopPx - viewYPx);

            // KEY FIX: Use the extent dimensions directly, not fixed tile size
            // The extent already accounts for any edge tiles that might be smaller
            float screenWidth = (float)pixelExtent.Width;
            float screenHeight = (float)pixelExtent.Height;

            // Comprehensive debug logging
            // Console.WriteLine(...) // Reduced noise for production

            return new TileRenderInfo(tile.Index, screenX, screenY, screenWidth, screenHeight);
        }

        public int CurrentLevel => _currentLevel;
        public int CachedTextureCount => _uploadedTiles.Count;
    }
}
