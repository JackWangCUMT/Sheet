using Sheet.Block.Core;
using Sheet.Item.Model;
using Sheet.Controller.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
    public class BlockSerializer : IBlockSerializer
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockHelper _blockHelper;
        private readonly IBlockFactory _blockFactory;

        public BlockSerializer(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockHelper = serviceLocator.GetInstance<IBlockHelper>();
            this._blockFactory = serviceLocator.GetInstance<IBlockFactory>();
        }

        #endregion

        #region Ids

        private int SetId(IBlock parent, int nextId)
        {
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    point.Id = nextId++;
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    line.Id = nextId++;
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    rectangle.Id = nextId++;
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    ellipse.Id = nextId++;
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    text.Id = nextId++;
                }
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    image.Id = nextId++;
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    block.Id = nextId++;
                    nextId = SetId(block, nextId);
                }
            }

            return nextId;
        }

        #endregion

        #region Serialize

        private ItemColor ToItemColor(byte a, byte r, byte g, byte b)
        {
            return new ItemColor()
            {
                Alpha = a,
                Red = r,
                Green = g,
                Blue = b
            };
        }

        private ItemColor ToItemColor(IArgbColor color)
        {
            if (color != null)
            {
                return new ItemColor()
                {
                    Alpha = color.Alpha,
                    Red = color.Red,
                    Green = color.Green,
                    Blue = color.Blue
                };
            }
            return null;
        }

        public PointItem Serialize(IPoint point)
        {
            var pointItem = new PointItem();

            pointItem.Id = point.Id;
            pointItem.X = _blockHelper.GetLeft(point);
            pointItem.Y = _blockHelper.GetTop(point);

            return pointItem;
        }

        public LineItem Serialize(ILine line)
        {
            var lineItem = new LineItem();

            lineItem.Id = line.Id;
            lineItem.X1 = _blockHelper.GetX1(line);
            lineItem.Y1 = _blockHelper.GetY1(line);
            lineItem.X2 = _blockHelper.GetX2(line);
            lineItem.Y2 = _blockHelper.GetY2(line);
            lineItem.Stroke = _blockHelper.GetStroke(line);
            lineItem.StartId = line.Start == null ? -1 : line.Start.Id;
            lineItem.EndId = line.End == null ? -1 : line.End.Id;

            return lineItem;
        }

        public RectangleItem Serialize(IRectangle rectangle)
        {
            var rectangleItem = new RectangleItem();

            rectangleItem.Id = rectangle.Id;
            rectangleItem.X = _blockHelper.GetLeft(rectangle);
            rectangleItem.Y = _blockHelper.GetTop(rectangle);
            rectangleItem.Width = _blockHelper.GetWidth(rectangle);
            rectangleItem.Height = _blockHelper.GetHeight(rectangle);
            rectangleItem.IsFilled = _blockHelper.IsTransparent(rectangle);
            rectangleItem.Stroke = _blockHelper.GetStroke(rectangle);
            rectangleItem.Fill = _blockHelper.GetFill(rectangle);

            return rectangleItem;
        }

        public EllipseItem Serialize(IEllipse ellipse)
        {
            var ellipseItem = new EllipseItem();

            ellipseItem.Id = ellipse.Id;
            ellipseItem.X = _blockHelper.GetLeft(ellipse);
            ellipseItem.Y = _blockHelper.GetTop(ellipse);
            ellipseItem.Width = _blockHelper.GetWidth(ellipse);
            ellipseItem.Height = _blockHelper.GetHeight(ellipse);
            ellipseItem.IsFilled = _blockHelper.IsTransparent(ellipse);
            ellipseItem.Stroke = _blockHelper.GetStroke(ellipse);
            ellipseItem.Fill = _blockHelper.GetFill(ellipse);

            return ellipseItem;
        }

        public TextItem Serialize(IText text)
        {
            var textItem = new TextItem();

            textItem.Id = text.Id;
            textItem.X = _blockHelper.GetLeft(text);
            textItem.Y = _blockHelper.GetTop(text);
            textItem.Width = _blockHelper.GetWidth(text);
            textItem.Height = _blockHelper.GetHeight(text);
            textItem.Text = _blockHelper.GetText(text);
            textItem.HAlign = _blockHelper.GetHAlign(text);
            textItem.VAlign = _blockHelper.GetVAlign(text);
            textItem.Size = _blockHelper.GetSize(text);
            textItem.Backgroud = _blockHelper.GetBackground(text);
            textItem.Foreground = _blockHelper.GetForeground(text);

            return textItem;
        }

        public ImageItem Serialize(IImage image)
        {
            var imageItem = new ImageItem();

            imageItem.Id = image.Id;
            imageItem.X = _blockHelper.GetLeft(image);
            imageItem.Y = _blockHelper.GetTop(image);
            imageItem.Width = _blockHelper.GetWidth(image);
            imageItem.Height = _blockHelper.GetHeight(image);
            imageItem.Data = _blockHelper.GetData(image);

            return imageItem;
        }

        public BlockItem Serialize(IBlock parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(parent.Id, parent.X, parent.Y, parent.Width, parent.Height, parent.DataId, parent.Name);
            blockItem.Backgroud = ToItemColor(parent.Backgroud);

            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    blockItem.Points.Add(Serialize(point));
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    blockItem.Lines.Add(Serialize(line));
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    blockItem.Rectangles.Add(Serialize(rectangle));
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    blockItem.Ellipses.Add(Serialize(ellipse));
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    blockItem.Texts.Add(Serialize(text));
                }
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    blockItem.Images.Add(Serialize(image));
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    blockItem.Blocks.Add(Serialize(block));
                }
            }

            return blockItem;
        }

        public BlockItem SerializerContents(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name)
        {
            var points = parent.Points;
            var lines = parent.Lines;
            var rectangles = parent.Rectangles;
            var ellipses = parent.Ellipses;
            var texts = parent.Texts;
            var images = parent.Images;
            var blocks = parent.Blocks;

            SetId(parent, id + 1);

            var sheet = new BlockItem() { Backgroud = ToItemColor(parent.Backgroud) };
            sheet.Init(id, x, y, width, height, dataId, name);

            if (points != null)
            {
                foreach (var point in points)
                {
                    sheet.Points.Add(Serialize(point));
                }
            }

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Lines.Add(Serialize(line));
                }
            }

            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Rectangles.Add(Serialize(rectangle));
                }
            }

            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Ellipses.Add(Serialize(ellipse));
                }
            }

            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Texts.Add(Serialize(text));
                }
            }

            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Images.Add(Serialize(image));
                }
            }

            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    sheet.Blocks.Add(Serialize(block));
                }
            }

            return sheet;
        }

        #endregion

        #region Deserialize

        public IPoint Deserialize(ISheet sheet, IBlock parent, PointItem pointItem, double thickness)
        {
            var point = _blockFactory.CreatePoint(thickness, pointItem.X, pointItem.Y, false);

            point.Id = pointItem.Id;

            if (parent != null)
            {
                parent.Points.Add(point);
            }

            if (sheet != null)
            {
                sheet.Add(point);
            }

            return point;
        }

        public ILine Deserialize(ISheet sheet, IBlock parent, LineItem lineItem, double thickness)
        {
            var line = _blockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2, lineItem.Stroke);

            line.Id = lineItem.Id;
            line.StartId = lineItem.StartId;
            line.EndId = lineItem.EndId;

            if (parent != null)
            {
                parent.Lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line);
            }

            return line;
        }

        public IRectangle Deserialize(ISheet sheet, IBlock parent, RectangleItem rectangleItem, double thickness)
        {
            var rectangle = _blockFactory.CreateRectangle(thickness,
                rectangleItem.X,
                rectangleItem.Y,
                rectangleItem.Width,
                rectangleItem.Height,
                rectangleItem.IsFilled,
                rectangleItem.Stroke,
                rectangleItem.Fill);

            rectangle.Id = rectangleItem.Id;

            if (parent != null)
            {
                parent.Rectangles.Add(rectangle);
            }

            if (sheet != null)
            {
                sheet.Add(rectangle);
            }

            return rectangle;
        }

        public IEllipse Deserialize(ISheet sheet, IBlock parent, EllipseItem ellipseItem, double thickness)
        {
            var ellipse = _blockFactory.CreateEllipse(thickness,
                ellipseItem.X,
                ellipseItem.Y,
                ellipseItem.Width,
                ellipseItem.Height,
                ellipseItem.IsFilled,
                ellipseItem.Stroke,
                ellipseItem.Fill);

            ellipse.Id = ellipseItem.Id;

            if (parent != null)
            {
                parent.Ellipses.Add(ellipse);
            }

            if (sheet != null)
            {
                sheet.Add(ellipse);
            }

            return ellipse;
        }

        public IText Deserialize(ISheet sheet, IBlock parent, TextItem textItem)
        {
            var text = _blockFactory.CreateText(textItem.Text,
                textItem.X, textItem.Y,
                textItem.Width, textItem.Height,
                textItem.HAlign,
                textItem.VAlign,
                textItem.Size,
                textItem.Backgroud,
                textItem.Foreground);

            text.Id = textItem.Id;

            if (parent != null)
            {
                parent.Texts.Add(text);
            }

            if (sheet != null)
            {
                sheet.Add(text);
            }

            return text;
        }

        public IImage Deserialize(ISheet sheet, IBlock parent, ImageItem imageItem)
        {
            var image = _blockFactory.CreateImage(imageItem.X, imageItem.Y, imageItem.Width, imageItem.Height, imageItem.Data);

            image.Id = imageItem.Id;

            if (parent != null)
            {
                parent.Images.Add(image);
            }

            if (sheet != null)
            {
                sheet.Add(image);
            }

            return image;
        }

        public IBlock Deserialize(ISheet sheet, IBlock parent, BlockItem blockItem, double thickness)
        {
            var block = _blockFactory.CreateBlock(blockItem.Id, blockItem.X, blockItem.Y, blockItem.Width, blockItem.Height, blockItem.DataId, blockItem.Name, blockItem.Backgroud);

            foreach (var textItem in blockItem.Texts)
            {
                Deserialize(sheet, block, textItem);
            }

            foreach (var imageItem in blockItem.Images)
            {
                Deserialize(sheet, block, imageItem);
            }

            foreach (var lineItem in blockItem.Lines)
            {
                Deserialize(sheet, block, lineItem, thickness);
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                Deserialize(sheet, block, rectangleItem, thickness);
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                Deserialize(sheet, block, ellipseItem, thickness);
            }

            foreach (var childBlockItem in blockItem.Blocks)
            {
                Deserialize(sheet, block, childBlockItem, thickness);
            }

            foreach (var pointItem in blockItem.Points)
            {
                Deserialize(sheet, block, pointItem, thickness);
            }

            if (parent != null)
            {
                parent.Blocks.Add(block);
            }

            return block;
        }

        #endregion
    }
}
