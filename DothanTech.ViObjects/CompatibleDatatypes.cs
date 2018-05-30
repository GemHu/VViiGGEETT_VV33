using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Dothan.ViObject
{
    public class CompatibleDatatypes
    {
        #region Life Cycle

        public CompatibleDatatypes(string compatibleDatatypesXMLFile)
        {
            this.BuildCompatibleTypeList(compatibleDatatypesXMLFile);
        }

        #endregion

        #region CompatibleTypesList

        public List<string> CompatibleTypesList
        {
            get
            {
                if (this._CompatibleTypesList == null)
                    this._CompatibleTypesList = new List<string>();

                return this._CompatibleTypesList;
            }
        }
        private List<string> _CompatibleTypesList;

        #endregion

        /// <summary>
        /// build a list of compatible datatypes 
        ///	TODO: the string like "int_to_word" is not regarded. if such block not available, 
        ///	the actual string should be found in the list during code generation.
        /// </summary>
        /// <param name="compatibleDatatypesFile"></param>
        public void BuildCompatibleTypeList(string compatibleDatatypesFile)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                if (File.Exists(compatibleDatatypesFile))
                    doc.Load(compatibleDatatypesFile);

                XmlNodeList inputList = doc.SelectNodes("//Input");

                foreach (XmlNode node in inputList)
                {
                    string inputType = node.Attributes["TypeName"].Value;

                    if (node.ChildNodes == null)
                        continue;

                    foreach (XmlNode item in node.ChildNodes)
                    {
                        string outputType = item.Attributes["TypeName"].Value;
                        string converterType = item.Attributes["ConverterName"].Value;

                        CompatibleTypesList.Add(inputType + "|" + outputType + "|" + converterType);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public bool AreTypesCompatible(ViDataType type1, ViDataType type2)
        {
            return this.AreTypesCompatible(type1.Name, type2.Name);
        }

        public bool AreTypesCompatible(string type1, string type2)
        {
            if (CompatibleTypesList.Count >= 0)
            {
                foreach (string item in CompatibleTypesList)
                {
                    int pos1 = item.IndexOf('|');
                    int pos2 = item.LastIndexOf('|');
                    string inputType = item.Substring(0, pos1);
                    string outputType = item.Substring(pos1 + 1, pos2 - pos1 - 1);

                    if (string.Compare(type1, inputType, true) == 0 && string.Compare(type2, outputType, true) == 0)
                        return true;
                }
            }

            if (!string.IsNullOrEmpty(type1) && !string.IsNullOrEmpty(type2))
                return string.Compare(type1, type2, true) == 0;

            return false;
        }
    }
}
