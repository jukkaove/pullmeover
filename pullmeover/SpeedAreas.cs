using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace PullMeOver
{
    public partial class PullMeOverMain : Script
    {
        internal static string[] paletostreets = new string[6]
{
      "Paleto Blvd",
      "Duluoz Ave",
      "Pyrite Ave",
      "Cascabel Ave",
      "Procopio Dr",
      "Procopio Promenade"
};
        internal static string[] grapseedstreets = new string[8]
{
      "O'Neil Way",
      "Joad Ln",
      "Grapeseed Main St",
      "Seaview Rd",
      "Union Rd",
      "Noth Calafia Way",
      "Grapeseed Ave",
      "East Joshua Road"
};
        internal static string[] sandyshoresstreets = new string[13]
{
      "Marina Dr",
      "Nilland Ave",
      "Cholla Springs Ave",
      "Armadillo Ave",
      "Algonquin Blvd",
      "Mountain View Dr",
      "Lolita Ave",
      "Panorama Dr",
      "Zancudo Ave",
      "Alhambra Dr",
      "Joshua Rd",
      "Lesbos Ln",
      "Meringue Ln"
};

        internal static string[] davisstreets = new string[8]
{
        "Brouge Avenue",
        "Carson Ave",
        "Covenant Ave",
        "Davis Ave",
        "Grove St",
        "Innocence Blvd",
        "Macdonald St",
        "Roy Lowenstein Blvd"
};

        internal static string[] delperrostreets = new string[14]
{
        "Bay City Ave",
        "Boulevard Del Perro",
        "Eastbourne Way",
        "Equality Way",
        "Hawick Ave",
        "Liberty St",
        "Magellan Ave",
        "North Rockford Dr",
        "Playa Vista",
        "Prosperity St",
        "Prosperity Street Promenade",
        "Red Desert Ave",
        "San Andreas Ave",
        "Sandcastle Way"
};

        internal static string[] rockfordstreets = new string[21]
{
        "Abe Milton Parkway",
        "Carcer Way",
        "Caesar Pl",
        "Dorset Dr",
        "Dorset Pl",
        "Dunstable Dr",
        "Dunstable Ln",
        "Edwood Way",
        "Greenwich Way",
        "Heritage Way ",
        "Mad Wayne Thunder Dr",
        "Marathon Ave",
        "Movie Star Way",
        "Portola Dr",
        "Rockford Dr",
        "San Vitus Blvd",
        "South Boulevard Del Perro",
        "South Mo Milton Dr",
        "South Rockford Dr",
        "Spanish Ave",
        "West Eclipse Blvd"
};

        internal static string[] lossantosstreets = new string[131]
{
        "Ace Jones Dr",
        "Adam's Apple Blvd",
        "Aguja St",
        "Alta Pl",
        "Alta St",
        "Amarillo Vista",
        "Amarillo Way",
        "Americano Way",
        "Atlee St",
        "Autopia Parkway",
        "Banham Canyon Drive",
        "Barbareno Road",
        "Bay City Incline",
        "Baytree Canyon Road",
        "Bridge St",
        "Buccaneer Way",
        "Buen Vino Road",
        "Caesars Pl",
        "Calais Ave",
        "Capital Blvd",
        "Chum St",
        "Chupacabra St",
        "Clinton Ave",
        "Cockingend Dr",
        "Conquistador St",
        "Cortes St",
        "Cougar Ave",
        "Cox Way",
        "Crusade Road",
        "Decker St",
        "Didion Dr",
        "Dry Dock St",
        "Dutch London St",
        "East Galileo Ave",
        "East Mirror Dr",
        "Eclipse Blvd",
        "Elgin Ave",
        "El Burro Blvd",
        "El Rancho Blvd",
        "Exceptionalists Way",
        "Fantastic Pl",
        "Fenwell Pl",
        "Forum Dr",
        "Fudge Ln",
        "Galileo Road",
        "Gentry Ln",
        "Ginger St",
        "Glory Way",
        "Goma St",
        "Greenwich Parkway",
        "Greenwich Pl",
        "Hanger Way",
        "Hangman Ave",
        "Hardy Way",
        "Hillcrest Ave",
        "Hillcrest Ridge Access Road",
        "Imagination Court",
        "Industry Passage",
        "Ineseno Road",
        "Integrity Way",
        "Invention Court",
        "Jamestown St",
        "Kimble Hill Dr",
        "Kortz Dr",
        "Labor Pl",
        "Laguna Pl",
        "Lake Vinewood Dr",
        "Las Lagunas Blvd",
        "Lindsay Circus",
        "Little Bighorn Ave",
        "Low Power St",
        "Marlowe Dr",
        "Melanoma St",
        "Meteor St",
        "Milton Road",
        "Mirror Park Blvd",
        "Mirror Pl",
        "Morningwood Blvd",
        "Mount Haan Dr",
        "Mount Haan Road",
        "Mount Vinewood Dr",
        "Mutiny Road",
        "New Empire Way",
        "Nikola Ave",
        "Nikola Pl",
        "Normandy Dr",
        "North Archer Ave",
        "North Conker Ave",
        "North Sheldon Ave",
        "Occupation Ave",
        "Orchardville Ave",
        "Palomino Ave",
        "Peaceful St",
        "Perth St",
        "Picture Perfect Dr",
        "Plaice Pl",
        "Popular St",
        "Power St",
        "Richman St",
        "Rub St",
        "Sam Austin Dr",
        "Senora Road",
        "Shank St",
        "Signal St",
        "Sinner St",
        "Sinners Passage",
        "South Arsenal St",
        "South Shambles St",
        "Steele Way",
        "Strangeways Dr",
        "Strawberry Ave",
        "Supply St",
        "Sustancia Road",
        "Swiss St",
        "Tackle St",
        "Tangerine St",
        "Tongva Dr",
        "Tower Way",
        "Tug St",
        "Utopia Gardens",
        "Vespucci Blvd",
        "Vinewood Blvd",
        "Vinewood Park Dr",
        "Vitus St",
        "Voodoo Pl",
        "West Galileo Ave",
        "West Mirror Dr",
        "Whispymound Dr",
        "Wild Oats Dr",
        "York St",
        "Zancudo Barranca "
};

        internal static string[] freeways = {
            "Los Santos Freeway",
            "Del Perro Freeway",
            "Great Ocean Highway",
            "La Puerta Freeway",
            "Elysian Fields Freeway",
            "Palomino Freeway",
            "Senora Freeway",
            "Los Santos Fwy",
            "Del Perro Fwy",
            "Great Ocean Highway",
            "Great Ocean Hwy",
            "La Puerta Fwy",
            "Elysian Fields Fwy",
            "Palomino Fwy",
            "Senora Fwy",
            "Olympic Fwy"

};

        internal static int paletospeedlimit;
        internal static int grapeseedspeedlimit;
        internal static int sandyshoresspeedlimit;
        internal static int delperrospeedlimit;
        internal static int davisspeedlimit;
        internal static int rockfordspeedlimit;
        internal static int lossantosspeedlimit;

        internal static string currentstreet = "";
        internal static string currentcrossstreet = "";

        internal static bool streetsloaded;

        void SetSpeedLimits()
        {
            paletospeedlimit = config.GetValue<int>("Speedlimits", "Paleto speed limit", 50);
            grapeseedspeedlimit = config.GetValue<int>("Speedlimits", "Grapeseed speed limit", 50);
            sandyshoresspeedlimit = config.GetValue<int>("Speedlimits", "Sandy Shores speed limit", 50);
            delperrospeedlimit = config.GetValue<int>("Speedlimits", "Del Perro speed limit", 50);
            davisspeedlimit = config.GetValue<int>("Speedlimits", "Davis speed limit", 50);
            rockfordspeedlimit = config.GetValue<int>("Speedlimits", "Rockford speed limit", 50);
            lossantosspeedlimit = config.GetValue<int>("Speedlimits", "Los Santos speed limit", 50);
            hwaylimit = config.GetValue<int>("Speedlimits", "Highway speed limit", 75);
            generallimit = config.GetValue<int>("Speedlimits", "General speed limit", 50);

            LoadStreetsFile();
        }

        void LoadStreetsFile()
        {   
            if (!File.Exists("./scripts/PullMeOver/Streets.xml"))
                return;

            
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(Directory.GetCurrentDirectory() + "./scripts/PullMeOver/Streets.xml");
 
                List<string> kadut = new List<string>();

                foreach (XmlElement node in doc.DocumentElement)
                {
                    kadut.Clear();
                    if(node.Name.Equals("Paleto"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        paletostreets = kadut.ToArray();
                    }                     
                    if (node.Name.Equals("Grapeseed"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        grapseedstreets = kadut.ToArray();
                    }                                   
                    if (node.Name.Equals("SandyShore"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        sandyshoresstreets = kadut.ToArray();
                    }                    
                    if (node.Name.Equals("DelPerro"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        delperrostreets = kadut.ToArray();
                    }                   
                    if (node.Name.Equals("Davis"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        davisstreets = kadut.ToArray();
                    }                   
                    if (node.Name.Equals("Rockford"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        rockfordstreets = kadut.ToArray();
                    }                   
                    if (node.Name.Equals("LosSantos"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        lossantosstreets = kadut.ToArray();
                    }                    
                    if (node.Name.Equals("Freeways"))
                    {
                        foreach (XmlNode item in node.ChildNodes)
                            kadut.Add(item.InnerText);
                        freeways = kadut.ToArray();
                    }
                }
                streetsloaded = true;
            }
            catch (Exception e)
            {

                UI.Notify("~r~Couldn't load Streets.xml file. ~w~"+e);
            }
        }

        internal static void SetStreet(bool valuechange)
        {

            OutputArgument streetname = new OutputArgument();
            OutputArgument crossingroad = new OutputArgument();

            Function.Call<bool>(Hash.GET_STREET_NAME_AT_COORD, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, streetname, crossingroad);
            string crossing = Function.Call<string>(Hash.GET_STREET_NAME_FROM_HASH_KEY, crossingroad.GetResult<int>());
            string street = Function.Call<string>(Hash.GET_STREET_NAME_FROM_HASH_KEY, streetname.GetResult<int>());

            if(!currentcrossstreet.Equals(crossing) || !currentstreet.Equals(street) || valuechange)
            {
                currentstreet = street;
                currentcrossstreet = crossing;

                if (freeways.Contains(street) || (freeways.Contains(crossing)))
                    speedlimit = SpeedMs(hwaylimit);
                else if (paletostreets.Contains(street))
                    speedlimit = SpeedMs(paletospeedlimit);
                else if (grapseedstreets.Contains(street))
                    speedlimit = SpeedMs(grapeseedspeedlimit);
                else if (sandyshoresstreets.Contains(street))
                    speedlimit = SpeedMs(sandyshoresspeedlimit);
                else if (davisstreets.Contains(street))
                    speedlimit = SpeedMs(davisspeedlimit);
                else if (delperrostreets.Contains(street))
                    speedlimit = SpeedMs(delperrospeedlimit);
                else if (rockfordstreets.Contains(street))
                    speedlimit = SpeedMs(rockfordspeedlimit);
                else if (lossantosstreets.Contains(street))
                    speedlimit = SpeedMs(lossantosspeedlimit);
                else
                    speedlimit = SpeedMs(generallimit);
            }

            
        }
    }
}
