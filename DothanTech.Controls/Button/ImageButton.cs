/// <summary>
/// @file   ImageButton.cs
///	@brief  带图片的 Button。
/// @author	DothanTech 吴桢楠
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dothan.Helpers;

/// <summary>
/// DothanTech 提供的公用控件。
/// </summary>
namespace Dothan.Controls
{
    /// <summary>
    /// 带图片的 Button。
    /// </summary>
    public class ImageButton : Button
    {
        public ImageButton()
        {
            this.Loaded += new RoutedEventHandler(ImageButton_Loaded);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(ImageButton_IsEnabledChanged);
        }

        void ImageButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_img != null)
                _img.IsEnabled = this.IsEnabled;
        }

        /// <summary>
        /// 图片缓存
        /// </summary>
        private EnableImage _img { get; set; }

        void ImageButton_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            ImageSource imgSource = null;
            try
            {
                if (MyImageSource != null && MyImageSource is ImageSource)
                {
                    imgSource = (ImageSource)MyImageSource;
                }
                else if (MyImageSource != null)
                {
                    imgSource = new BitmapImage(new Uri(MyImageSource.ToString(), UriKind.RelativeOrAbsolute));

                }
                _img = new EnableImage() { Source = imgSource, Width = 16 };
                
                sp.Children.Add(_img);
            }
            catch
            { }

            try
            {
                sp.Children.Add(new TextBlock() { Text = MyText, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            }
            catch { }

            this.Content = sp;
        }

        public static DependencyProperty ImageSourceProperty =
           DependencyProperty.Register("MyImageSource", typeof(ImageSource), typeof(ImageButton),
          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// 图片源、地址
        /// </summary>
        public ImageSource MyImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static DependencyProperty TextProperty =
          DependencyProperty.Register("MyText", typeof(string), typeof(ImageButton),
         new FrameworkPropertyMetadata(null));
        /// <summary>
        /// Text内容
        /// </summary>
        public string MyText
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
