using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Xml;

namespace Dothan.Helpers
{
    public class DZProperty
    {
        public string Name = "";
        public bool ReadOnly = false;
        public string Description = "";
        public string Value = "";
        public string _DisplayName = "";
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(_DisplayName) ? Name : _DisplayName; }
            set { _DisplayName = value; }
        }
        public string Category = "";
        public HWAddress Address = null;
        public string DataType = "";
        public int DownloadOffset = 0;
        public string ValueList = "";

        public void Serialize(XmlWriter writer, bool includeReadOnly)
        {
            writer.WriteStartElement("property");
            {
                writer.WriteAttributeString("name", Name);
                if (includeReadOnly || ReadOnly == true)
                    writer.WriteAttributeString("readOnly", ReadOnly.ToString());
                //
                writer.WriteElementString("description", Description);
                if (!string.IsNullOrEmpty(ValueList))
                {
                    writer.WriteElementString("valueList", ValueList);
                }
                writer.WriteElementString("value", Value);
                writer.WriteElementString("displayName", _DisplayName);
                writer.WriteElementString("category", Category);
                if (Address != null)
                {
                    writer.WriteStartElement("address");
                    {
                        if (!string.IsNullOrEmpty(Address.Category))
                            writer.WriteAttributeString("category", Address.Category);
                        writer.WriteElementString("allowedCPUTypes", Address.AllowedCPUTypes);
                        writer.WriteElementString("allowedFunctionBlockTypes", Address.AllowedFunctionBlockTypes);
                        writer.WriteElementString("allowedVMESlots", Address.AllowedVMESlots);
                        writer.WriteElementString("vmeBusAddress", Address.VmeBusAddress);
                        writer.WriteElementString("uniqueAddressID", Address.UniqueAddressID);
                    }
                    writer.WriteEndElement();
                }
                if (DownloadOffset > 0)
                {
                    writer.WriteStartElement("downloadRelevant");
                    {
                        writer.WriteAttributeString("dataType", DataType);
                        writer.WriteAttributeString("offset", DownloadOffset.ToString());
                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public DZProperty Clone()
        {
            DZProperty prop = this.MemberwiseClone() as DZProperty;
            if (prop.Address != null) prop.Address = prop.Address.Clone();
            return prop;
        }

        #region Custom Property Shown Support

        protected Dictionary<string, string> ParseValueList(string valueList)
        {
            if (string.IsNullOrEmpty(valueList))
                return null;

            valueList = valueList.Trim();
            string[] list = valueList.Split(';', '；');
            if (list == null || list.Count() <= 0)
                return null;

            Dictionary<string, string> DValueList =
                new Dictionary<string, string>(list.Count());
            foreach (string val in list)
            {
                string[] vals = val.Split(':', '：');
                if (vals == null) continue;

                switch (vals.Count())
                {
                    case 1:
                        DValueList[vals[0].Trim()] = vals[0].Trim();
                        break;
                    case 2:
                        DValueList[vals[0].Trim()] = vals[1].Trim();
                        break;
                }
            }

            return DValueList;
        }

        public void CreatePropertyDescriptors(ArrayList props, bool specific)
        {
            string displayName = DisplayName;
            string description = Description;
            if (DownloadOffset > 0)
            {
                string compileInfo = string.Format("Data Type: {0}, Download Offset: {1}",
                                                                DataType, DownloadOffset);
                if (string.IsNullOrEmpty(description))
                    description = compileInfo;
                else
                    description = compileInfo + "\r\n" + description;
            }

            PropertyDescriptor basePD = null;
            TypeConverter converter = null;
            if (!string.IsNullOrEmpty(ValueList))
            {
                basePD = DZPropertyContainer.GetEnumPD();
                Dictionary<string, string> dic = ParseValueList(ValueList);
                converter = new DZEnumConverter(dic);
            }
            else if (string.Compare(DataType, "BOOL", true) == 0)
            {
                basePD = DZPropertyContainer.GetBoolPD();
            }
            else
            {
                basePD = DZPropertyContainer.GetStringPD();
            }

            ArrayList attrs = DZPropertyDescriptor.CreateAttributes(displayName, description,
                string.IsNullOrEmpty(Category) ? (specific ? "Specific" : "Common") : Category);

            DZModulePropertyDescriptor pd =
                new DZModulePropertyDescriptor(Name, basePD, attrs, ReadOnly);
            if (converter != null) pd.SetConverter(converter);
            // add pd into list
            props.Add(pd);

            // build pd for Address
            if (Address != null)
            {
                string addressCategory = string.IsNullOrEmpty(Address.Category) ? displayName : Address.Category;
                basePD = DZPropertyContainer.GetStringPD();
                //
                attrs = DZPropertyDescriptor.CreateAttributes("Allowed CPU Type(s)", "Allowed CPU Type(s) of " + displayName, addressCategory);
                props.Add(new DZModulePropertyDescriptor(GetAddressPropName("allowedCPUTypes"), basePD, attrs, true));
                attrs = DZPropertyDescriptor.CreateAttributes("Allowed FB Type(s)", "Allowed Function Block Type(s) of " + displayName, addressCategory);
                props.Add(new DZModulePropertyDescriptor(GetAddressPropName("allowedFunctionBlockTypes"), basePD, attrs, true));
                attrs = DZPropertyDescriptor.CreateAttributes("Allowed VME Slot(s)", "Allowed VME Slot(s) of " + displayName, addressCategory);
                props.Add(new DZModulePropertyDescriptor(GetAddressPropName("allowedVMESlots"), basePD, attrs, true));
                attrs = DZPropertyDescriptor.CreateAttributes("VME Bus Address", "VME Bus Address of " + displayName, addressCategory);
                props.Add(new DZModulePropertyDescriptor(GetAddressPropName("vmeBusAddress"), basePD, attrs, true));
                attrs = DZPropertyDescriptor.CreateAttributes("Unique Address ID", "Unique Address ID of " + displayName, addressCategory);
                props.Add(new DZModulePropertyDescriptor(GetAddressPropName("uniqueAddressID"), basePD, attrs, true));
            }
        }

        public string GetAddressValue(string addressName)
        {
            if (Address == null) return null;
            if (string.IsNullOrEmpty(addressName)) return null;

            switch (addressName)
            {
                case "allowedCPUTypes":
                    return Address.AllowedCPUTypes;
                case "allowedFunctionBlockTypes":
                    return Address.AllowedFunctionBlockTypes;
                case "allowedVMESlots":
                    return Address.AllowedVMESlots;
                case "vmeBusAddress":
                    return Address.VmeBusAddress;
                case "uniqueAddressID":
                    return Address.UniqueAddressID;
                default:
                    return null;
            }
        }

        public void SetAddressValue(string addressName, string value)
        {
            if (Address == null) return;
            if (string.IsNullOrEmpty(addressName)) return;

            switch (addressName)
            {
                case "allowedCPUTypes":
                    Address.AllowedCPUTypes = value;
                    break;
                case "allowedFunctionBlockTypes":
                    Address.AllowedFunctionBlockTypes = value;
                    break;
                case "allowedVMESlots":
                    Address.AllowedVMESlots = value;
                    break;
                case "vmeBusAddress":
                    Address.VmeBusAddress = value;
                    break;
                case "uniqueAddressID":
                    Address.UniqueAddressID = value;
                    break;
            }
        }

        public string GetAddressPropName(string addressName)
        {
            return GetAddressPropName(Name, addressName);
        }

        static public string GetAddressPropName(string propName, string addressName)
        {
            return string.Format("``{0}``{1}", propName, addressName);
        }

        static public bool SepAddressPropName(string name, ref string propName, ref string addressName)
        {
            if (name.Length < 6) return false;
            if (string.Compare(name, 0, "``", 0, 2, true) != 0)
                return false;

            int pos = name.IndexOf("``", 2 + 1);
            if (pos < 0) return false;

            propName = name.Substring(2, pos - 2);
            addressName = name.Substring(pos + 2);
            return true;
        }

        #endregion
    }

    public class DZModulePropertyDescriptor : DZPropertyDescriptor
    {
        protected string propertyName = "";
        public static DZModulePropertyDescriptor CreateStringPropertyDescriptor(string propName,
            string displayName, string description, string category, bool readOnly)
        {
            ArrayList attrs = CreateAttributes(displayName, description, category);
            PropertyDescriptor pd = DZPropertyContainer.GetStringPD();
            return new DZModulePropertyDescriptor(propName, pd, attrs, readOnly);
        }

        public DZModulePropertyDescriptor(string propName,
            PropertyDescriptor pd, ArrayList attrs, bool readOnly)
            : base(pd, attrs)
        {
            propertyName = propName;
            SetReadOnly(readOnly);
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            DZModule module = component as DZModule;
            if (module == null) return null;
            return module.GetValue(propertyName);
        }

        public override void SetValue(object component, object value)
        {
            DZModule module = component as DZModule;
            if (module == null) return;
            module.SetValue(propertyName, value);
        }
    }

    public class HWAddress
    {
        public string Category = "";
        public string AllowedCPUTypes = "";
        public string AllowedFunctionBlockTypes = "";
        public string AllowedVMESlots = "";
        public string VmeBusAddress = "";
        public string UniqueAddressID = "";

        public HWAddress Clone()
        {
            return this.MemberwiseClone() as HWAddress;
        }
    }

    public interface DZModule
    {
        string GetValue(string propertyName);

        void SetValue(string propertyName, object value);
    }
}
