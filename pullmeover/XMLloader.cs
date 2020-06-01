using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class XMLloader
{
    static List<int> addonlista = new List<int>();

    public static void LoadXml()
    {
        string polku = Directory.GetCurrentDirectory();
        int curVal;
        // instantiate XmlDocument and load XML from file
        XmlDocument doc = new XmlDocument();
        doc.Load(polku+ "\\scripts\\PullMeOverAddonPeds.xml");

        // get a list of nodes - in this case, I'm selecting all <AID> nodes under
        // the <GroupAIDs> node - change to suit your needs
        XmlNodeList aNodes = doc.SelectNodes("/AddonPeds/ModelHash");

        // loop through all AID nodes
        foreach (XmlNode aNode in aNodes)
        {
            // grab the "id" attribute
            XmlAttribute idAttribute = aNode.Attributes["value"];

            // check if that attribute even exists...
            if (idAttribute != null)
            {
                // if yes - read its current value
                string currentValue = idAttribute.Value;


                curVal = Int32.Parse(currentValue, System.Globalization.CultureInfo.CurrentCulture);

                addonlista.Add(curVal);
                //                     Console.WriteLine(curVal.ToString("0.000000"));
                //                     Console.ReadKey();
                // here, you can now decide what to do - for demo purposes,
                // I just set the ID value to a fixed value if it was empty before
                //   if (string.IsNullOrEmpty(currentValue))
                //   {
                // curVal2.ToString();
                //   }
            }
        }

        // save the XmlDocument back to disk
    }

    public List<int> getPedilista()
    {
        return addonlista;
    }

    public string Polku()
    {
        return Directory.GetCurrentDirectory() + "\\scripts";
    }
}


