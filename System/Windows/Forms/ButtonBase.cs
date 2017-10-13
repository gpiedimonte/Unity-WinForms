﻿namespace System.Windows.Forms
{
    using System.Drawing;

    public class ButtonBase : Control
    {
        internal Color uwfBorderColor = SystemColors.ActiveBorder;
        internal Color uwfBorderDisableColor = Color.FromArgb(217, 217, 217);
        internal Color uwfBorderHoverColor = Color.FromArgb(126, 180, 234);
        internal Color uwfBorderSelectColor = SystemColors.Highlight;
        internal Color uwfDisableColor = SystemColors.Control;
        internal Color uwfHoverColor = Color.FromArgb(223, 238, 252);
        internal Bitmap uwfImageHover;
        internal Color uwfImageColor = Color.White;
        internal Color uwfImageHoverColor = Color.White;

        private const float IMAGE_BORDER_OFFSET = 2f;

        private readonly Pen borderPen = new Pen(Color.Transparent);

        private FlatStyle flatStyle = FlatStyle.Standard;
        private Bitmap imageFromList;
        private ContentAlignment imageAlign = ContentAlignment.MiddleCenter;
        private int imageIndex;
        private ImageList imageList;
        private ContentAlignment textAlign = ContentAlignment.MiddleCenter;

        public ButtonBase()
        {
            BackgroundImageLayout = ImageLayout.Center;
        }

        public FlatStyle FlatStyle
        {
            get { return flatStyle; }
            set { flatStyle = value; }
        }
        public Bitmap Image { get; set; }
        public ContentAlignment ImageAlign
        {
            get { return imageAlign; }
            set { imageAlign = value; }
        }
        public int ImageIndex
        {
            get { return imageIndex; }
            set
            {
                imageIndex = value;
                if (ImageList != null && imageIndex >= 0 && imageIndex < ImageList.Images.Count)
                    imageFromList = ImageList.Images[imageIndex] as Bitmap;
            }
        }
        public ImageList ImageList
        {
            get { return imageList; }
            set
            {
                imageList = value;
                if (ImageList != null && imageIndex >= 0 && imageIndex < ImageList.Images.Count)
                    imageFromList = ImageList.Images[imageIndex] as Bitmap;
            }
        }
        public virtual ContentAlignment TextAlign
        {
            get { return textAlign; }
            set { textAlign = value; }
        }

        protected override Size DefaultSize
        {
            get { return new Size(75, 23); }
        }

        internal override void RaiseOnMouseClick(MouseEventArgs e)
        {
            var iButtonControl = this as IButtonControl;
            if (iButtonControl != null)
            {
                OnMouseClick(e);
                iButtonControl.PerformClick(); // Originally in WndProc.
            }
            else
                base.RaiseOnMouseClick(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Return)
                OnClick(EventArgs.Empty);

            base.OnKeyUp(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            var enabled = Enabled;
            var height = Height;
            var width = Width;

            var backColorToDraw = Color.Transparent;
            var borderColorToDraw = Color.Transparent;

            // Get colors from style.
            switch (flatStyle)
            {
                case FlatStyle.Flat:
                    if (uwfHovered) backColorToDraw = Color.FromArgb(32, 0, 0, 0);
                    borderColorToDraw = Color.Black;
                    break;
                case FlatStyle.Popup:
                    borderColorToDraw = Color.FromArgb(85, 85, 85); // TODO: border width for top-left corner.
                    break;
                case FlatStyle.Standard:
                case FlatStyle.System:
                    // Back.
                    if (enabled)
                        backColorToDraw = uwfHovered == false ? BackColor : uwfHoverColor;
                    else
                        backColorToDraw = uwfDisableColor;

                    // Border.
                    if (enabled == false)
                        borderColorToDraw = uwfBorderDisableColor;
                    else if (uwfHovered)
                        borderColorToDraw = uwfBorderHoverColor;
                    else if (Focused)
                        borderColorToDraw = uwfBorderSelectColor;
                    else
                        borderColorToDraw = uwfBorderColor;
                    break;
            }

            borderPen.Color = borderColorToDraw;

            g.uwfFillRectangle(backColorToDraw, 0, 0, width, height);
            g.DrawRectangle(borderPen, 0, 0, width, height);
            DrawBackgroundImage(g);
            DrawImage(g);

            // Text.
            var textColor = ForeColor;
            if (enabled == false) textColor = SystemColors.InactiveCaption;
            var padding = Padding;
            g.uwfDrawString(Text, Font, textColor,
                    padding.Left,
                    padding.Top,
                    width - padding.Horizontal,
                    height - padding.Vertical, textAlign);
        }

        private void DrawBackgroundImage(Graphics g)
        {
            // Background image.
            if (BackgroundImage != null)
            {
                var backgroundImage = BackgroundImage;
                var imageColorToPaint = uwfImageColor;
                if (uwfHovered)
                {
                    if (uwfImageHover != null)
                        backgroundImage = uwfImageHover;
                    imageColorToPaint = uwfImageHoverColor;
                }

                // Fix: wrong colors.
                switch (BackgroundImageLayout)
                {
                    default:
                    case ImageLayout.None:
                        g.uwfDrawImage(backgroundImage, imageColorToPaint, 0, 0, backgroundImage.Width, backgroundImage.Height);
                        break;
                    case ImageLayout.Center:
                        g.uwfDrawImage(backgroundImage, imageColorToPaint,
                            Width / 2 - backgroundImage.Width / 2,
                            Height / 2 - backgroundImage.Height / 2,
                            backgroundImage.Width,
                            backgroundImage.Height);
                        break;
                    case ImageLayout.Stretch:
                        g.uwfDrawImage(backgroundImage, imageColorToPaint, 0, 0, Width, Height);
                        break;
                    case ImageLayout.Zoom:
                        // TODO: not working.
                        break;
                }
            }
        }
        private void DrawImage(Graphics g)
        {
            // Image.
            var imageToPaint = Image;
            if (imageFromList != null)
                imageToPaint = imageFromList;

            if (imageToPaint == null) return;

            var imageWidth = imageToPaint.Width;
            var imageHeight = imageToPaint.Height;
            var imageColorToPaint = uwfImageColor;
            if (uwfHovered)
            {
                if (uwfImageHover != null)
                    imageToPaint = uwfImageHover;
                imageColorToPaint = uwfImageHoverColor;
            }

            float ix = 0, iy = 0;

            switch (imageAlign)
            {
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomRight:
                    iy = Height - imageHeight - IMAGE_BORDER_OFFSET;
                    break;
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleRight:
                    iy = Height / 2f - imageHeight / 2f;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopRight:
                    iy = IMAGE_BORDER_OFFSET;
                    break;
            }

            switch (imageAlign)
            {
                case ContentAlignment.BottomCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.TopCenter:
                    ix = Width / 2f - imageWidth / 2f;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.TopLeft:
                    ix = IMAGE_BORDER_OFFSET;
                    break;
                case ContentAlignment.BottomRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.TopRight:
                    ix = Width - imageWidth - IMAGE_BORDER_OFFSET;
                    break;
            }

            g.uwfDrawImage(imageToPaint, imageColorToPaint, ix, iy, imageWidth, imageHeight);
        }
    }
}
