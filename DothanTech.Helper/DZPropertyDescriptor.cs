using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Dothan.Helpers
{
    public class DZPropertyDescriptor : PropertyDescriptor
    {
        public static ArrayList CreateAttributes(string displayName, string description, string category)
        {
            ArrayList attrs = new ArrayList(3);

            if (!string.IsNullOrEmpty(displayName))
            {
                attrs.Add(new DisplayNameAttribute(displayName));
            }

            if (!string.IsNullOrEmpty(description))
            {
                attrs.Add(new DescriptionAttribute(description));
            }

            if (!string.IsNullOrEmpty(category))
            {
                attrs.Add(new CategoryAttribute(category));
            }

            return attrs;
        }

        protected PropertyDescriptor _pd = null;
        protected bool _readOnly = false;
        protected TypeConverter _tc = null;

        public DZPropertyDescriptor(PropertyDescriptor pd)
            : base(pd)
        {
            _pd = pd;
        }
        public DZPropertyDescriptor(PropertyDescriptor pd, ArrayList attrs)
            : base(pd, attrs.ToArray(typeof(Attribute)) as Attribute[])
        {
            _pd = pd;

            foreach (object obj in attrs)
            {
                if (obj is ReadOnlyAttribute)
                {
                    _readOnly = ((ReadOnlyAttribute)obj).IsReadOnly;
                    break;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {{{1}}}", this.DisplayName, this.Category);
        }

        public virtual void SetReadOnly(bool readOnly)
        {
            _readOnly = readOnly;
        }

        public virtual void SetConverter(TypeConverter tc)
        {
            _tc = tc;
        }

        public override Type ComponentType
        {
            get { return _pd.ComponentType; }
        }

        public override TypeConverter Converter
        {
            get { return _tc ?? _pd.Converter; }
        }

        public override bool IsLocalizable
        {
            get { return _pd.IsLocalizable; }
        }

        public override bool IsReadOnly
        {
            get { return _readOnly; }
        }

        public override Type PropertyType
        {
            get { return _pd.PropertyType; }
        }

        public override bool SupportsChangeEvents
        {
            get { return _pd.SupportsChangeEvents; }
        }

        public override bool CanResetValue(object component)
        {
            return _pd.CanResetValue(component);
        }

        public override object GetValue(object component)
        {
            return _pd.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            _pd.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            _pd.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _pd.ShouldSerializeValue(component);
        }
    }

    [TypeConverter(typeof(DZEnumConverter))]
    public enum __DZEnum
    {
        dummy
    }
    public class DZEnumConverter : EnumConverter
    {
        public DZEnumConverter()
            : base(typeof(__DZEnum))
        {
        }
        public DZEnumConverter(Dictionary<string, string> keyValue)
            : base(typeof(__DZEnum))
        {
            SetKeyValue(keyValue);
        }

        public virtual void SetKeyValue(Dictionary<string, string> keyValue)
        {
            Key2Value = keyValue;

            if (Key2Value != null)
            {
                Values = new TypeConverter.StandardValuesCollection(Key2Value.Values);

                Value2Key = new Dictionary<string, string>(Key2Value.Count);
                foreach (KeyValuePair<string, string> kvp in Key2Value)
                {
                    Value2Key[kvp.Value] = kvp.Key;
                }
            }
            else
            {
                Values = new TypeConverter.StandardValuesCollection(new ArrayList());
                Value2Key = null;
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null) return null;

            if (value is string && Key2Value != null)
            {
                string str = value as string;
                if (Key2Value.ContainsKey(str))
                    return Key2Value[str];
            }

            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null) return null;

            if (destinationType == typeof(string) && Value2Key != null)
            {
                string str = value as string;
                if (Value2Key.ContainsKey(str))
                    return Value2Key[str];
            }

            return null;
        }

        protected Dictionary<string, string> Key2Value = null;
        protected Dictionary<string, string> Value2Key = null;
    }

    public class DZPropertyContainer
    {
        public string __string
        {
            get { return null; }
            set { }
        }

        private static PropertyDescriptor _stringPD = null;
        public static PropertyDescriptor GetStringPD()
        {
            if (_stringPD != null) return _stringPD;

            PropertyDescriptorCollection pdc =
                TypeDescriptor.GetProperties(typeof(DZPropertyContainer));

            _stringPD = pdc["__string"];
            return _stringPD;
        }

        public bool __bool
        {
            get { return false; }
            set { }
        }

        private static PropertyDescriptor _boolPD = null;
        public static PropertyDescriptor GetBoolPD()
        {
            if (_boolPD != null) return _boolPD;

            PropertyDescriptorCollection pdc =
                TypeDescriptor.GetProperties(typeof(DZPropertyContainer));

            _boolPD = pdc["__bool"];
            return _boolPD;
        }

        public __DZEnum __enum
        {
            get { return __DZEnum.dummy; }
            set { }
        }

        private static PropertyDescriptor _enumPD = null;
        public static PropertyDescriptor GetEnumPD()
        {
            if (_enumPD != null) return _enumPD;

            PropertyDescriptorCollection pdc =
                TypeDescriptor.GetProperties(typeof(DZPropertyContainer));

            _enumPD = pdc["__enum"];
            return _enumPD;
        }
    }

    public class DZTypeDescriptor : ICustomTypeDescriptor
    {
        private PropertyDescriptorCollection _pdc = null;
        public DZTypeDescriptor()
        {
        }
        public DZTypeDescriptor(PropertyDescriptorCollection pdc)
        {
            _pdc = pdc;
        }
        public DZTypeDescriptor(PropertyDescriptor[] pdc)
        {
            _pdc = new PropertyDescriptorCollection(pdc);
        }
        public DZTypeDescriptor(PropertyDescriptor[] pdc, bool readOnly)
        {
            _pdc = new PropertyDescriptorCollection(pdc, readOnly);
        }
        public DZTypeDescriptor(ArrayList pdc)
        {
            _pdc = new PropertyDescriptorCollection(
                pdc.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[]);
        }
        public DZTypeDescriptor(ArrayList pdc, bool readOnly)
        {
            _pdc = new PropertyDescriptorCollection(
                pdc.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[], readOnly);
        }

        [Browsable(false)]
        public PropertyDescriptorCollection PropertyDescriptors
        {
            get { return _pdc; }
            set { _pdc = value; }
        }

        public virtual AttributeCollection GetAttributes()
        {
            AttributeCollection col = TypeDescriptor.GetAttributes(this, true);
            return col;
        }

        public virtual string GetClassName()
        {
            return GetType().FullName;
        }

        public virtual string GetComponentName()
        {
            return null;
        }

        public virtual TypeConverter GetConverter()
        {
            TypeConverter tc = TypeDescriptor.GetConverter(this, true);
            return tc;
        }

        public virtual EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public virtual PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public virtual object GetEditor(Type editorBaseType)
        {
            object o = TypeDescriptor.GetEditor(this, editorBaseType, true);
            return o;
        }

        public virtual EventDescriptorCollection GetEvents()
        {
            return null;
        }

        public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return null;
        }

        public virtual PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection pcol = GetProperties(null);
            return pcol;
        }

        public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (_pdc != null)
                return _pdc;

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this, attributes, true);
            return props;
        }

        public virtual object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
