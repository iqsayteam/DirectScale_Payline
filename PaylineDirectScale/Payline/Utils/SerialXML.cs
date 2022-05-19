using System;
using System.IO;
using System.Xml.Serialization;

namespace PaylineDirectScale.Payline.Utils
{
    public class SerialXML
    {
        public static bool Save(string Dir, string FileName, object classIn)
        {
            FileStream fs = null;
            try
            {
                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);

                string XMLFile = Path.Combine(Dir, FileName);

                fs = new FileStream(XMLFile, FileMode.Create, FileAccess.Write);

                XmlSerializer ser = new XmlSerializer(classIn.GetType());
                ser.Serialize(fs, classIn);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        public static object Load(string Dir, string FileName, Type typeIn)
        {
            FileStream fs = null;
            try
            {
                string XMLFile = Path.Combine(Dir, FileName);

                if (!File.Exists(XMLFile))
                {
                    return Activator.CreateInstance(typeIn, null);
                }

                fs = new FileStream(XMLFile, FileMode.Open, FileAccess.Read);
                XmlSerializer ser = new XmlSerializer(typeIn);
                object o = ser.Deserialize(fs);

                return o;
            }
            catch
            {
                return Activator.CreateInstance(typeIn, null);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
    }
}
