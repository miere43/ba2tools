using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.ComponentModel;

namespace Ba2Explorer.Settings.Serializer
{
    public static class SettingsXmlSerializer
    {
        static Type serializeSettingsPropertyType = typeof(SerializeSettingsPropertyAttribute);

        static Dictionary<Type, Func<object, string>> customSerializers;

        static Dictionary<Type, Func<string, object>> customDeserializers;

        static SettingsXmlSerializer()
        {
            var invariant = System.Globalization.CultureInfo.InvariantCulture;

            customSerializers = new Dictionary<Type, Func<object, string>>()
            {
                { typeof(string), (obj) => ((string)obj) },
                { typeof(int), (obj) => ((int)obj).ToString(invariant.NumberFormat) },
                { typeof(float), (obj) => ((float)obj).ToString(invariant.NumberFormat) }
            };

            customDeserializers = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(string), (str) => str },
                { typeof(int), (str) => Convert.ToInt32(str, invariant.NumberFormat) },
                { typeof(float), (str) => float.Parse(str, invariant.NumberFormat) }
            };
        }

        private static Func<object, string> GetCustomSerializerForType(Type type)
        {
            Func<object, string> serializer = null;
            if (customSerializers.TryGetValue(type, out serializer))
                return serializer;
            return null;
        }

        private static Func<string, object> GetCustomDeserializerForType(Type type)
        {
            Func<string, object> deserializer = null;
            if (customDeserializers.TryGetValue(type, out deserializer))
                return deserializer;
            return null;
        }

        private static CustomAttributeTypedArgument GetAttributeDefaultValueArgument(CustomAttributeData attribute)
        {
            return attribute.NamedArguments.First((na) => na.MemberName == "DefaultValue").TypedValue;
        }

        public static bool Serialize<T>(T settingsInstance, Stream output)
        {
            Contract.Requires(settingsInstance != null);
            Contract.Requires(output != null);

            Type settingsType = typeof(T);
            var properties = settingsType.GetProperties();

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Document,
            };

            using (XmlWriter writer = XmlWriter.Create(output))
            {
                writer.WriteStartDocument(true);
                writer.WriteWhitespace(Environment.NewLine);
                writer.WriteStartElement("Settings");
                writer.WriteWhitespace(Environment.NewLine);

                foreach (var property in properties)
                {
                    CustomAttributeData serializeAttribute =
                        property.CustomAttributes.FirstOrDefault(
                            (ca) => ca.AttributeType == serializeSettingsPropertyType);

                    if (serializeAttribute != null)
                    {
                        string propertyName = property.Name;
                        Type propertyValueType = property.PropertyType;
                        object propertyRawValue = property.GetMethod.Invoke(settingsInstance, null);
                        //if (propertyRawValue == null)
                        //{
                        //    var defaultTypedValue = GetAttributeDefaultValueArgument(serializeAttribute);
                        //    object defaultValue = defaultTypedValue.Value;
                        //    propertyValueType = defaultTypedValue.ArgumentType;

                        //    propertyRawValue = defaultValue;
                        //}

                        string value;
                        if (propertyRawValue == null)
                        {
                            value = "null";
                        }
                        else
                        {
                            var typeConverterAttribute = propertyValueType.GetCustomAttribute(typeof(TypeConverterAttribute));
                            if (typeConverterAttribute == null)
                            {
                                var serializer = GetCustomSerializerForType(propertyValueType);
                                if (serializer == null)
                                {

                                    value = propertyRawValue.ToString();
                                }
                                else
                                {
                                    value = serializer.Invoke(propertyRawValue);
                                }
                            }
                            else
                            {
                                var typeConverterAttr = typeConverterAttribute as TypeConverterAttribute;
                                var typeConverter = Type.GetType(typeConverterAttr.ConverterTypeName, true, true);
                                TypeConverter typeConverterInstance = 
                                    (TypeConverter)typeConverter.GetConstructor(new Type[0]).Invoke(new object[0]);

                                if (typeConverterInstance.CanConvertTo(typeof(string)))
                                {
                                    value = typeConverterInstance.ConvertToInvariantString(propertyRawValue);
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }
                        }

                        writer.WriteWhitespace("\t");
                        writer.WriteElementString(propertyName, value);
                        writer.WriteWhitespace(Environment.NewLine);
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return true;
        }

        public static T Deserialize<T>(Stream input, out IList<SettingsDeserializationError> errors)
        {
            Type settingsType = typeof(T);
            var properties = settingsType.GetProperties();

            T settingsInstance = (T)settingsType.GetConstructor(new Type[0]).Invoke(new object[0]);
            XDocument document = null;
            XElement settingsXElement = null;
            try
            {
                if (input != null)
                {
                    document = XDocument.Load(input, LoadOptions.PreserveWhitespace);
                    settingsXElement = document.Element("Settings");
                }
            }
            catch
            {
                document = null;
                settingsXElement = null;
            }

            errors = new List<SettingsDeserializationError>(0);

            foreach (var property in properties)
            {
                CustomAttributeData serializeAttribute =
                    property.CustomAttributes.FirstOrDefault(
                        (ca) => ca.AttributeType == serializeSettingsPropertyType);

                if (serializeAttribute != null)
                {
                    Type propertyValueType = property.PropertyType;
                    bool useDefaultValue = document == null;
                    XElement propertyXElement = null;

                    string propertyName = property.Name;
                    object propertyValue = null;
                    if (document != null && !useDefaultValue)
                    {
                        try
                        {
                            propertyXElement = settingsXElement.Element(propertyName);
                        } catch { propertyXElement = null; }

                        if (propertyXElement == null)
                            useDefaultValue = true;
                        else
                        {
                            var typeConverterAttribute = propertyValueType.GetCustomAttribute(typeof(TypeConverterAttribute));
                            bool useCustomDeserializer = false;
                            if (typeConverterAttribute == null)
                            {
                                useCustomDeserializer = true;
                            }
                            
                            if (typeConverterAttribute != null && !useCustomDeserializer) {
                                var typeConverterAttr = typeConverterAttribute as TypeConverterAttribute;
                                var typeConverter = Type.GetType(typeConverterAttr.ConverterTypeName, true, true);
                                TypeConverter typeConverterInstance =
                                    (TypeConverter)typeConverter.GetConstructor(new Type[0]).Invoke(new object[0]);

                                if (typeConverterInstance.CanConvertFrom(typeof(string)))
                                {
                                    propertyValue = typeConverterInstance.ConvertFromInvariantString(propertyXElement.Value);
                                }
                                else
                                {
                                    useCustomDeserializer = true;
                                }
                            }

                            if (useCustomDeserializer)
                            {
                                var deserializer = GetCustomDeserializerForType(propertyValueType);
                                if (deserializer == null)
                                {
                                    throw new NotImplementedException($"No deserializer for type {propertyValueType.ToString()} supported.");
                                }
                                try
                                {
                                    propertyValue = deserializer.Invoke(propertyXElement.Value);
                                }
                                catch (Exception e)
                                {
                                    errors.Add(new SettingsDeserializationError(propertyName, e));
                                    useDefaultValue = true;
                                }
                            }
                        }
                    }

                    if (useDefaultValue)
                    {
                        // leave as is
                    }
                    else
                    {
                        property.SetValue(settingsInstance, propertyValue);
                    }
                }
            }

            return settingsInstance;
        }
    }
}
