using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace Dothan.ViObject
{
    public class ViFileHeader : ViObject, IViSerializeBlur
    {
        #region life cycle

        public ViFileHeader()
        {

        }

        #endregion

        #region D CompanyName

        public static readonly DependencyProperty CompanyNameProperty =
            DependencyProperty.Register("CompanyName", typeof(string), typeof(ViFileHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string CompanyName
        {
            get { return (string)GetValue(CompanyNameProperty); }
            set { SetValue(CompanyNameProperty, value); }
        }

        #endregion

        #region D CompanyURL

        public static readonly DependencyProperty CompanyURLProperty =
            DependencyProperty.Register("CompanyURL", typeof(string), typeof(ViFileHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string CompanyURL
        {
            get { return (string)GetValue(CompanyURLProperty); }
            set { SetValue(CompanyURLProperty, value); }
        }

        #endregion

        #region D ProductName

        public static readonly DependencyProperty ProductNameProperty =
            DependencyProperty.Register("ProductName", typeof(string), typeof(ViFileHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string ProductName
        {
            get { return (string)GetValue(ProductNameProperty); }
            set { SetValue(ProductNameProperty, value); }
        }

        #endregion

        #region D ProductVersion

        public static readonly DependencyProperty ProductVersionProperty =
            DependencyProperty.Register("ProductVersion", typeof(string), typeof(ViFileHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string ProductVersion
        {
            get { return (string)GetValue(ProductVersionProperty); }
            set { SetValue(ProductVersionProperty, value); }
        }

        #endregion

        #region D ProductRelease

        public static readonly DependencyProperty ProductReleaseProperty =
            DependencyProperty.Register("ProductRelease", typeof(string), typeof(ViFileHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string ProductRelease
        {
            get { return (string)GetValue(ProductReleaseProperty); }
            set { SetValue(ProductReleaseProperty, value); }
        }

        #endregion

        #region D CreationDateTime

        public static readonly DependencyProperty CreationDateTimeProperty =
            DependencyProperty.Register("CreationDateTime", typeof(DateTime), typeof(ViFileHeader),
                                    new FrameworkPropertyMetadata(null));

        public DateTime CreationDateTime
        {
            get { return (DateTime)GetValue(CreationDateTimeProperty); }
            set { SetValue(CreationDateTimeProperty, value); }
        }

        #endregion

        #region D ContentDescription

        public static readonly DependencyProperty ContentDescriptionProperty =
            DependencyProperty.Register("ContentDescription", typeof(string), typeof(ViFileHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string ContentDescription
        {
            get { return (string)GetValue(ContentDescriptionProperty); }
            set { SetValue(ContentDescriptionProperty, value); }
        }

        #endregion

        #region IViSerializeBlur Members

        public bool SerializeTo(ViBlur blur, System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("fileHeader");
            {
                if (this.CompanyName != null)
                    writer.WriteAttributeString("companyName", this.CompanyName);
                if (this.CompanyURL != null)
                    writer.WriteAttributeString("companyURL", this.CompanyURL);
                if (this.ProductName != null)
                    writer.WriteAttributeString("productName", this.ProductName);
                if (this.ProductVersion != null)
                    writer.WriteAttributeString("productVersion", this.ProductVersion);
                if (this.ProductRelease != null)
                    writer.WriteAttributeString("productRelease", this.ProductRelease);
                if (this.CreationDateTime != null)
                    writer.WriteAttributeString("creationDateTime", this.CreationDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                if (this.ContentDescription != null)
                    writer.WriteAttributeString("contentDescription", this.ContentDescription);
            }
            writer.WriteEndElement();

            return true;
        }

        public bool DeserializeFrom(ViBlur blur, XmlReader reader)
        {
            this.CompanyName = reader.GetAttribute("companyName");
            this.CompanyURL = reader.GetAttribute("companyURL");
            this.ProductName = reader.GetAttribute("productName");
            this.ProductVersion = reader.GetAttribute("productVersion");
            this.ProductRelease = reader.GetAttribute("productRelease");
            string createTime = reader.GetAttribute("creationDateTime");
            this.ContentDescription = reader.GetAttribute("contentDescription");

            if (!string.IsNullOrEmpty(createTime))
            {
                try { this.CreationDateTime = DateTime.Parse(createTime); }
                catch (Exception) { this.CreationDateTime = DateTime.Now; }
            }

            // 空标签退出，否则读取子标签
            if (reader.IsEmptyElement)
                return true;

            while (!reader.EOF)
            {
                if (!reader.Read()) break;
                if (reader.NodeType == XmlNodeType.Whitespace) continue;

                if (reader.NodeType == XmlNodeType.Element)
                {

                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (string.Compare(reader.Name, "fileHeader", true) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
