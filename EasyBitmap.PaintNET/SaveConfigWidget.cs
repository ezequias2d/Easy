using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Easy.Paint.NET.Plugin
{
    internal partial class EasyBitmapSaveConfigWidget : SaveConfigWidget<EasyBitmapPlugin, EasyBitmapSaveConfigToken>
    {
        private ComboBox compression;
        private ComboBox filter;
        private ComboBox pixelOrder;

        public EasyBitmapSaveConfigWidget()
            : base(new EasyBitmapPlugin())
        {
            filter = new ComboBox
            {
                AutoSize = true
            };

            compression = new ComboBox
            {
                AutoSize = true
            };

            pixelOrder = new ComboBox
            {
                AutoSize = true,
                Text = "PixelOrder"
            };

            pixelOrder.Items.Add(PixelFormat.ARGB);
            pixelOrder.Items.Add(PixelFormat.RGBA);
            pixelOrder.Items.Add(PixelFormat.GrayScale);
            pixelOrder.Items.Add(PixelFormat.RGB);
            pixelOrder.Items.Add(PixelFormat.GrayScaleAlpha);

            compression.Items.Add("None");
            compression.Items.Add("Deflate");
            compression.Items.Add("LZ4");
            compression.Items.Add("RLE");
            compression.Items.Add("EasyLZ");

            filter.Items.Add("None");
            filter.Items.Add("Axis");
            filter.Items.Add("Sub");

            compression.SelectedIndexChanged += tokenChanged;
            pixelOrder.SelectedIndexChanged += tokenChanged;
            filter.SelectedIndexChanged += tokenChanged;

            Label label1 = new Label();
            label1.Text = "PixelOrder";
            Label label2 = new Label();
            label2.Text = "Compression";
            Label label3 = new Label();
            label3.Text = "Filter";


            pixelOrder.Location = new Point(pixelOrder.Location.X, pixelOrder.Location.Y + label1.Size.Height + 3);

            label2.Location = new Point(label2.Location.X, pixelOrder.Location.Y + pixelOrder.Size.Height + 3);

            compression.Location = new Point(compression.Location.X, label2.Location.Y + label2.Size.Height + 3);

            label3.Location = new Point(label3.Location.X, compression.Location.Y + compression.Size.Height + 3);

            filter.Location = new Point(filter.Location.X, label3.Location.Y + label3.Size.Height + 3);

            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(pixelOrder);
            Controls.Add(compression);
            Controls.Add(filter);

            filter.SelectedIndex = 1;
            compression.SelectedIndex = 1;
            pixelOrder.SelectedIndex = 0;
        }

        protected override EasyBitmapSaveConfigToken CreateTokenFromWidget()
        {
            return new EasyBitmapSaveConfigToken
            {
                Compression = (uint)compression.SelectedIndex,
                PixelOrder = (PixelFormat)pixelOrder.SelectedIndex,
                Filter = (uint)filter.SelectedIndex
            };
        }

        protected override void InitWidgetFromToken(EasyBitmapSaveConfigToken sourceToken)
        {
            compression.SelectedIndex = (int)sourceToken.Compression;
            pixelOrder.SelectedIndex = (int)sourceToken.PixelOrder;
            filter.SelectedIndex = (int)sourceToken.Filter;
        }

        private void tokenChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }
    }
}
