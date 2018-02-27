public class ImageDesigner : IDisposable, IImageDesigner
    {
        private Stream data { get; set; }

        private Size oryginalSize;

        public Size Size { get; set; }

        public string FileName { get; set; }

        private ImageFactory convertingFactory;


        public ImageDesigner(Stream data, string fileName)
        {
            this.data = data;
            this.FileName = fileName;
            this.convertingFactory = new ImageFactory(preserveExifData: true);
            this.convertingFactory.Load(data);
            this.oryginalSize = this.GetSizeFromStream();
            this.Size = oryginalSize;   //Get initation size from stream data,
        }

        public void ChangeFormat(MimeType type)
        {
            ISupportedImageFormat format;

            switch (type.Title)
            {
                case "image/bmp":
                    format = new BitmapFormat(); break;
                case "image/jpeg":
                    format = new JpegFormat(); break;
                case "image/png":
                    format = new PngFormat(); break;
                case "image/tiff":
                    format = new TiffFormat(); break;
                case "image/gif":
                    format = new GifFormat(); break;
                default:
                    throw new ArgumentException("Image format is not supported.");
            }

            this.FileName = Path.ChangeExtension(this.FileName, type.Extension);

            ChangeFormat(format);
        }

        public void ChangeFormat(ISupportedImageFormat format)
        {
            convertingFactory.Format(format);
        }

        public void SetSize(Size size)
        {
            this.Size = size;

            var config = new ResizeLayer(size, ResizeMode.Stretch);
            this.convertingFactory.Resize(config);
        }

        public void SetScaledSize(Size percentageSize)
        {
            var newWidth = PictureFileTools.GetRelativeValue(this.oryginalSize.Width, percentagetSize.Width);
            var newHeight = PictureFileTools.GetRelativeValue(this.oryginalSize.Height, percentagetSize.Height);

            SetSize(new Size(newWidth, newHeight));
        }

        private Size GetSizeFromStream()
        {
            return Image.FromStream(this.data).Size;
        }

        public static Size GetRelativeSize(Size value, int percent)
        {
            return new Size(GetRelativeLength(value.Width, percent), GetRelativeLength(value.Width, percent));
        }

        public static int GetRelativeLength(int value, int percent)
        {
            return value * percent / 100;
        }

        public void SetWatermark(Stream watermark, Point position, Size size)
        {
            //if(Image.FromStream(watermark).Size != size)
            var resizedWatermark = new MemoryStream();

            var resizeLayer = new ResizeLayer(size, ResizeMode.Stretch);

            using (var watermarkFactory = new ImageFactory(preserveExifData: true))
            {
                watermarkFactory.Load(watermark)
                    .Resize(resizeLayer)
                    .Save(resizedWatermark);
            }
            

            var watermarkConfig = new ImageLayer
            {
                Image = Image.FromStream(resizedWatermark),
                Position = position,
                Size = size,
                Opacity = 100
            };

            this.convertingFactory.Overlay(watermarkConfig);
        }

        public Stream ExecuteConversion()
        {
            //Save to new stream
            Stream result = new MemoryStream();
            this.convertingFactory.Save(result);

            return result;
        }

        public void Dispose()
        {
            this.convertingFactory.Dispose();
            this.data.Dispose();
        }
    }
}