/// <summary>
/// @file   EnableImage.cs
///	@brief  支持 Enable / Disable 两种状态时，图片灰度显示的 Image 控件。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Dothan.Helpers
{
    /// <summary>
    /// 支持 Enable / Disable 两种状态时，图片灰度显示的 Image 控件。
    /// </summary>
    public class EnableImage : Image
    {
        #region Disabled Source

        public static readonly DependencyProperty DisabledSourceProperty =
            DependencyProperty.Register("DisabledSource", typeof(ImageSource), typeof(EnableImage),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnDisabledSourceChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Common")]
        [Description("Image Source if control is disabled")]
        public ImageSource DisabledSource
        {
            get { return (ImageSource)GetValue(DisabledSourceProperty); }
            set { SetValue(DisabledSourceProperty, value); }
        }

        /// <summary>
        /// 根据指定的 Enable 图像，自动生成对象的 Enable 图像和 Disable 图像。
        /// </summary>
        public void GenerateSources(ImageSource enabledSource)
        {
            this.EnabledSource = enabledSource;
            if (enabledSource == null)
            {
                this.DisabledSource = null;
            }
            else if (enabledSource is BitmapSource)
            {
                try
                {
                    // 图像灰处理。因为缺省的 FormatConvertedBitmap.Gray8 不支持 Alpha 通道，因此
                    // 只能自己写函数来转换成灰度图像了。
                    BitmapSource source = enabledSource as BitmapSource;

                    // 将图像转换成 BGRA32 的固定格式
                    if (!source.Format.Equals(PixelFormats.Bgra32))
                    {
                        FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
                        newFormatedBitmapSource.BeginInit();
                        newFormatedBitmapSource.Source = source;
                        newFormatedBitmapSource.DestinationFormat = PixelFormats.Bgra32;
                        newFormatedBitmapSource.EndInit();
                        //
                        source = newFormatedBitmapSource;
                    }

                    // 灰度化的三个颜色分量的权值
                    const int RedPart = (int)(0.299 * 65536);
                    const int GreenPart = (int)(0.587 * 65536);
                    const int BluePart = (65536 - RedPart - GreenPart);

                    // 得到图像的点阵数据
                    int stride = source.PixelWidth * 4;
                    byte[] pixels = new byte[stride * source.PixelHeight];
                    source.CopyPixels(pixels, stride, 0);
                    // 循环转换所有的点
                    for (int j = 0, offset = 0; j < source.PixelHeight; ++j)
                    {
                        for (int i = 0; i < source.PixelWidth; ++i, offset += 4)
                        {
                            byte gray = (byte)((pixels[offset + 2] * BluePart +
                                                pixels[offset + 1] * GreenPart +
                                                pixels[offset + 0] * RedPart +
                                                65536 / 2) / 65536);
                            pixels[offset + 0] =
                            pixels[offset + 1] =
                            pixels[offset + 2] = gray;
                        }
                    }

                    // 创建新图像
                    source = BitmapImage.Create(source.PixelWidth, source.PixelHeight,
                        source.DpiX, source.DpiY, source.Format, source.Palette, pixels, stride);
                    //
                    this.DisabledSource = source;
                }
                catch (Exception)
                {
                    this.DisabledSource = null;
                }
            }
            else
            {
                this.DisabledSource = null;
            }

            this.UpdateSource();
        }

        #endregion

        #region Source Management

        /// <summary>
        /// 根据当前使能状态，更新对象的 Image Source。
        /// </summary>
        public void UpdateSource()
        {
            if (!this.IsEnabled && this.DisabledSource != null)
                this.Source = this.DisabledSource;
            else
                this.Source = this.EnabledSource;
        }

        protected ImageSource EnabledSource = null;

        #endregion

        #region Class lifecycle

        static EnableImage()
        {
            SnapsToDevicePixelsProperty.OverrideMetadata(typeof(EnableImage), new FrameworkPropertyMetadata(true));
            //UseLayoutRoundingProperty.OverrideMetadata(typeof(EnableImage), new FrameworkPropertyMetadata(false));
            RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(EnableImage), new FrameworkPropertyMetadata(EdgeMode.Aliased));

            SourceProperty.OverrideMetadata(typeof(EnableImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSourceChanged)));
            IsEnabledProperty.OverrideMetadata(typeof(EnableImage), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsEnabledChanged)));
        }

        protected static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EnableImage This = d as EnableImage;
            if (!e.NewValue.Equals(This.DisabledSource))
                This.GenerateSources(e.NewValue as ImageSource);
        }

        protected static void OnDisabledSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EnableImage This = d as EnableImage;
            This.UpdateSource();
        }

        protected static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EnableImage This = d as EnableImage;
            This.UpdateSource();
        }

        #endregion
    }
}
