using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace Dothan.ViObject
{
    public class ViContentHeader : ViObject, IViSerializeBlur
    {
        #region Life Cycle

        public ViContentHeader()
        {

        }

        #endregion

        #region D Name

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        #endregion

        #region D Version

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        #endregion

        #region D ModificationDateTime

        public static readonly DependencyProperty ModificationDateTimeProperty =
            DependencyProperty.Register("ModificationDateTime", typeof(DateTime), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata(null));

        public DateTime ModificationDateTime
        {
            get { return (DateTime)GetValue(ModificationDateTimeProperty); }
            set { SetValue(ModificationDateTimeProperty, value); }
        }

        #endregion

        #region D Organization

        public static readonly DependencyProperty OrganizationProperty =
            DependencyProperty.Register("Organization", typeof(string), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata("DothanTech"));

        public string Organization
        {
            get { return (string)GetValue(OrganizationProperty); }
            set { SetValue(OrganizationProperty, value); }
        }

        #endregion

        #region D Author

        public static readonly DependencyProperty AuthorProperty =
            DependencyProperty.Register("Author", typeof(string), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata(Environment.UserName));

        public string Author
        {
            get { return (string)GetValue(AuthorProperty); }
            set { SetValue(AuthorProperty, value); }
        }

        #endregion

        #region D Language

        public static readonly DependencyProperty LanguageProperty =
            DependencyProperty.Register("Language", typeof(string), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata("zh-cn"));

        public string Language
        {
            get { return (string)GetValue(LanguageProperty); }
            set { SetValue(LanguageProperty, value); }
        }

        #endregion

        #region D CoordinateInfo

        public static readonly DependencyProperty CoordinateInfoProperty =
            DependencyProperty.Register("CoordinateInfo", typeof(string), typeof(ViContentHeader),
                                        new FrameworkPropertyMetadata(string.Empty));

        public string CoordinateInfo
        {
            get { return (string)GetValue(CoordinateInfoProperty); }
            set { SetValue(CoordinateInfoProperty, value); }
        }

        #endregion

        #region IViSerializeBlur Members

        public bool SerializeTo(ViBlur blur, XmlWriter writer)
        {
            writer.WriteStartElement("contentHeader");
            {
                writer.WriteAttributeString("name", this.Name);
                writer.WriteAttributeString("version", this.Version);
                writer.WriteAttributeString("modificationDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteAttributeString("organization", this.Organization);
                writer.WriteAttributeString("author", this.Author);
                writer.WriteAttributeString("language", this.Language);

                writer.WriteStartElement("comment");
                {
                    writer.WriteString(this.Comment);
                }
                writer.WriteEndElement();

                writer.WriteStartElement("coordinateInfo");
                {

                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            return true;
        }

        public bool DeserializeFrom(ViBlur blur, XmlReader reader)
        {
            this.Name = reader.GetAttribute("name");
            this.Version = reader.GetAttribute("version");
            string modifyTime = reader.GetAttribute("modificationDateTime");
            this.Organization = reader.GetAttribute("organization");
            this.Author = reader.GetAttribute("author");
            this.Language = reader.GetAttribute("language");

            if (!string.IsNullOrEmpty(modifyTime))
            {
                try { this.ModificationDateTime = DateTime.Parse(modifyTime); }
                catch (Exception) { this.ModificationDateTime = DateTime.Now; }
            }

            if (reader.IsEmptyElement)
                return true;

            while (!reader.EOF)
            {
                if (!reader.Read()) break;
                if (reader.NodeType == XmlNodeType.Whitespace) continue;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (string.Compare(reader.Name, "comment", true) == 0)
                    {
                        if (!reader.Read()) return false;
                        this.Comment = reader.Value;
                    }
                    else if (string.Compare(reader.Name, "coordinateInfo", true) == 0)
                    {

                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (string.Compare(reader.Name, "contentHeader", true) == 0)
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
