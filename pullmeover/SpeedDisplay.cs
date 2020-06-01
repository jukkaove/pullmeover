using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Drawing;
using System.Windows.Forms;
using NativeUI;

namespace PullMeOver
{
    public class SpeedDisplay : Script
    {
        UIMenu mymenu;
        MenuPool mymenupool;
        List<dynamic> speeds = new List<dynamic> { "20", "25", "30", "35", "40", "45", "50", "55", "60",
        "65", "70", "75", "80", "85", "90", "95", "100", "105", "110", "115", "120", "125", "130", "135", "140", "145", "150"};
        List<dynamic> pointsx = new List<dynamic>();
        List<dynamic> pointsy = new List<dynamic>();
        List<dynamic> rgb = new List<dynamic>();
        List<dynamic> scalelist = new List<dynamic>();

        UIMenuListItem xposition;
        UIMenuListItem yposition;
        UIMenuListItem scales;
        UIMenuListItem red;
        UIMenuListItem green;
        UIMenuListItem blue;

        private UIText speedText;
        private Color textcolor;
        float speed;
        float speedlimit;
        int speedunit;
        int speedlimitunit;
        int r = 200;
        int g = 200;
        int b = 200;
        //float p = 0;
        float wlimit = PullMeOverMain.wlimit;
        float scale;
        Point point1;

        bool showspeed = true;
        bool showunit = true;
        bool showdisplay = true;
        bool showlimit = true;
        bool hidewhenscriptdisabled = true;

        ScriptSettings config;

        UIMenu submenu;
        UIMenu submenu2;

        UIMenuListItem paleto;
        UIMenuListItem grapeseed;
        UIMenuListItem sandyshores;
        UIMenuListItem delperro;
        UIMenuListItem davis;
        UIMenuListItem rockford;
        UIMenuListItem lossantos;
        UIMenuListItem general;
        UIMenuListItem highway;

        static Vehicle playervehicle;

        int aikanyt;

        public SpeedDisplay()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            SetDisplay();
        }

        void SetDisplay()
        {
            aikanyt = DateTime.Now.Second;
            Interval = 1;
            config = ScriptSettings.Load("scripts/PullMeOver.ini");
            point1.X = config.GetValue<int>("UI", "X", 1120);
            point1.Y = config.GetValue<int>("UI", "Y", 180);

            r = config.GetValue<int>("UI", "Red", 200);
            g = config.GetValue<int>("UI", "Green", 200);
            b = config.GetValue<int>("UI", "Blue", 200);

            PullMeOverMain.scriptenabled = config.GetValue<bool>("Settings", "enabled", true);
            showdisplay = config.GetValue<bool>("UI", "show hud", true);
            showspeed = config.GetValue<bool>("UI", "show speed", true);
            showunit = config.GetValue<bool>("UI", "show unit", true);
            hidewhenscriptdisabled = config.GetValue<bool>("UI", "Hide hud when sript is disabled", false);

            scale = config.GetValue<float>("UI", "Size", 0.5f);

            for (int i = 0; i < UI.WIDTH; i++) pointsx.Add(i);
            for (int i = 0; i < UI.HEIGHT; i++) pointsy.Add(i);
            for (int i = 0; i <= 255; i++) rgb.Add(i);
            for (int i = 1; i <= 10; i++) scalelist.Add(i*0.1f);

            mymenupool = new MenuPool();
            mymenu = new UIMenu("PullMeOver", "");
            xposition = new UIMenuListItem("X Position", pointsx, point1.X, "Hold Shift to scroll faster");
            yposition = new UIMenuListItem("Y Position", pointsy, point1.Y, "Hold Shift to scroll faster");
            scales = new UIMenuListItem("Size", scalelist, (int)(scale*10-1), "Hold Shift to scroll faster");

            red = new UIMenuListItem("Red", rgb, r, "Hold Shift to scroll faster");
            green = new UIMenuListItem("Green", rgb, g, "Hold Shift to scroll faster");
            blue = new UIMenuListItem("Blue", rgb, b, "Hold Shift to scroll faster");

            mymenu.AddItem(new UIMenuCheckboxItem("Enabled", PullMeOverMain.scriptenabled));
            submenu2 = new UIMenu("HUD Options", "Hud options");
            submenu2 = mymenupool.AddSubMenu(mymenu, "HUD Options");
            submenu = new UIMenu("Speed Limits", "speeds");

            submenu = mymenupool.AddSubMenu(mymenu, "Speed Limits", "Change speed limits");
            mymenu.AddItem(new UIMenuItem("Show record", ""));
            
            
            paleto = new UIMenuListItem("Paleto", speeds, GetAreaSpeed("paleto"));
            grapeseed = new UIMenuListItem("Grapeseed", speeds, GetAreaSpeed("grapeseed"));
            sandyshores = new UIMenuListItem("Sandyshores", speeds, GetAreaSpeed("sandyshores"));
            delperro = new UIMenuListItem("Del Perro", speeds, GetAreaSpeed("delperro"));
            davis = new UIMenuListItem("Davis", speeds, GetAreaSpeed("davis"));
            rockford = new UIMenuListItem("Rockford", speeds, GetAreaSpeed("rockford"));
            lossantos = new UIMenuListItem("Los Santos", speeds, GetAreaSpeed("lossantos"));
            highway = new UIMenuListItem("Highway", speeds, GetAreaSpeed("highway"));
            general = new UIMenuListItem("General", speeds, GetAreaSpeed("general"));
            submenu.AddItem(paleto);
            submenu.AddItem(grapeseed);
            submenu.AddItem(sandyshores);
            submenu.AddItem(delperro);
            submenu.AddItem(davis);
            submenu.AddItem(rockford);
            submenu.AddItem(lossantos);
            submenu.AddItem(highway);
            submenu.AddItem(general);

            submenu2.AddItem(new UIMenuCheckboxItem("HUD", showdisplay));
            submenu2.AddItem(new UIMenuCheckboxItem("Hide HUD when script is disabled", hidewhenscriptdisabled));
            submenu2.AddItem(new UIMenuCheckboxItem("Show speed", showspeed));
            submenu2.AddItem(new UIMenuCheckboxItem("Show unit", showunit));
            submenu2.AddItem(xposition);
            submenu2.AddItem(yposition);
            submenu2.AddItem(scales);
            submenu2.AddItem(red);
            submenu2.AddItem(green);
            submenu2.AddItem(blue);

            if(PullMeOverMain.streetsloaded)
            {
                var check = new UIMenuItem("Streets loaded", "Streets.xml file was successfully loaded");
                check.SetLeftBadge(UIMenuItem.BadgeStyle.Tick);
                mymenu.AddItem(check);
            }          
            
            mymenupool.Add(mymenu);
            mymenupool.RefreshIndex();

            submenu2.OnItemSelect += ItemSelectHandler;
            submenu2.OnListChange += ListChangeHandler;
            submenu2.OnCheckboxChange += CheckboxChangeHandler;
            submenu.OnItemSelect += ItemSelectHandler;
            submenu.OnListChange += ListChangeHandler;
            submenu.OnCheckboxChange += CheckboxChangeHandler;
            mymenu.OnCheckboxChange += CheckboxChangeHandler;
            mymenu.OnItemSelect += ItemSelectHandler;
        }
  
        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == PullMeOverMain.enablescript && !mymenupool.IsAnyMenuOpen())
            {
                mymenu.Visible = !mymenu.Visible;
            }
            if (e.KeyCode == Keys.Left && submenu2.Visible && e.Shift)
            {
                submenu2.GoLeft();
                submenu2.GoLeft();
            }
            if (e.KeyCode == Keys.Right && submenu2.Visible && e.Shift)
            {
                submenu2.GoRight();
                submenu2.GoRight();
            }
        }
        public void CheckboxChangeHandler(UIMenu sender, UIMenuItem selectedItem, bool Checked)
        {
            switch (selectedItem.Text)
            {
                case "Enabled":
                    PullMeOverMain.scriptenabled = !PullMeOverMain.scriptenabled;
                    config.SetValue("Settings", "enabled", BoolState(PullMeOverMain.scriptenabled));
                    /*if (PullMeOverMain.scriptenabled)
                        UI.Notify("PullMeOver ~g~ENABLED");
                    else
                        UI.Notify("PullMeOver ~r~DISABLED");*/
                    break;
                case "Show speed":
                    showspeed = !showspeed;
                    config.SetValue("UI", "show speed", BoolState(showspeed));
                    break;
                case "Show unit":
                    showunit = !showunit;
                    config.SetValue("UI", "show unit", BoolState(showunit));
                    break;
                case "HUD":
                    showdisplay = !showdisplay;
                    config.SetValue("UI", "show hud", BoolState(showdisplay));
                    break;
                case "Hide HUD when script is disabled":
                    hidewhenscriptdisabled = !hidewhenscriptdisabled;
                    config.SetValue("UI", "Hide hud when sript is disabled", BoolState(hidewhenscriptdisabled));
                    break;
            }
            config.Save();
        }

        bool BoolState(bool value)
        {
            if (value)
                return true;
            return false;
        }
        public void ItemSelectHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
           switch (selectedItem.Text)
            {
                case "Show record":
                    PullMeOverMain.ShowRecord();
                    break;
            }
        }

        public void ListChangeHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {  
            switch (selectedItem.Text)
            {
                case "Size":
                    scale = scales.IndexToItem(index);
                    config.SetValue<float>("UI", "size", scales.IndexToItem(index));
                    break;
                case "X Position":
                    point1.X = xposition.IndexToItem(index);
                    config.SetValue<int>("UI", "X", index);
                    break;
                case "Y Position":
                    point1.Y = yposition.IndexToItem(index);
                    config.SetValue<int>("UI", "Y", index);
                    break;
                case "Red":
                    r = red.IndexToItem(index);
                    config.SetValue<int>("UI", "Red", index);
                    break;
                case "Green":
                    g = green.IndexToItem(index);
                    config.SetValue<int>("UI", "Green", index);
                    break;
                case "Blue":
                    b = blue.IndexToItem(index);
                    config.SetValue<int>("UI", "Blue", index);
                    break;

                case "Paleto":                  
                    PullMeOverMain.paletospeedlimit = Int32.Parse(paleto.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Paleto speed limit", Int32.Parse(paleto.IndexToItem(index)));
                    //   speedlimitunit = PullMeOverMain.SpeedConversion(speedlimit);
                    PullMeOverMain.SetStreet(true);
                    break;
                case "Grapeseed":
                    PullMeOverMain.grapeseedspeedlimit = Int32.Parse(grapeseed.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Grapeseed speed limit", Int32.Parse(grapeseed.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;
                case "Sandyshores":
                    PullMeOverMain.sandyshoresspeedlimit = Int32.Parse(sandyshores.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Sandy Shores speed limit", Int32.Parse(sandyshores.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;    
                case "Del Perro":
                    PullMeOverMain.delperrospeedlimit = Int32.Parse(delperro.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Del Perro speed limit", Int32.Parse(delperro.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;
                case "Davis":
                    PullMeOverMain.davisspeedlimit = Int32.Parse(davis.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Davis speed limit", Int32.Parse(davis.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;
                case "Rockford":
                    PullMeOverMain.rockfordspeedlimit = Int32.Parse(rockford.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Rockford speed limit", Int32.Parse(rockford.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;
                case "Los Santos":
                    PullMeOverMain.lossantosspeedlimit = Int32.Parse(lossantos.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Los Santos speed limit", Int32.Parse(lossantos.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;
                case "Highway":
                    PullMeOverMain.hwaylimit = Int32.Parse(highway.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "Highway speed limit", Int32.Parse(highway.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;
                case "General":
                    PullMeOverMain.generallimit = Int32.Parse(general.IndexToItem(index));
                    config.SetValue<int>("Speedlimits", "General speed limit", Int32.Parse(general.IndexToItem(index)));
                    PullMeOverMain.SetStreet(true);
                    break;

            }
            config.Save();
        }
        int GetAreaSpeed(string area)
        {
            switch (area)
            {
                case "paleto":
                    if (speeds.Contains(PullMeOverMain.paletospeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.paletospeedlimit.ToString());
                    return 6;
                case "grapeseed":
                    if (speeds.Contains(PullMeOverMain.grapeseedspeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.grapeseedspeedlimit.ToString());
                    return 6;
                case "sandyshores":
                    if (speeds.Contains(PullMeOverMain.sandyshoresspeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.sandyshoresspeedlimit.ToString());
                    return 6;
                case "delperro":
                    if (speeds.Contains(PullMeOverMain.delperrospeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.delperrospeedlimit.ToString());
                    return 6;
                case "davis":
                    if (speeds.Contains(PullMeOverMain.davisspeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.davisspeedlimit.ToString());
                    return 6;
                case "rockford":
                    if (speeds.Contains(PullMeOverMain.rockfordspeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.rockfordspeedlimit.ToString());
                    return 6;
                case "lossantos":
                    if (speeds.Contains(PullMeOverMain.lossantosspeedlimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.lossantosspeedlimit.ToString());
                    return 6;
                case "highway":
                    if (speeds.Contains(PullMeOverMain.hwaylimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.hwaylimit.ToString());
                    return 6;
                case "general":
                    if (speeds.Contains(PullMeOverMain.generallimit.ToString()))
                        return speeds.IndexOf(PullMeOverMain.generallimit.ToString());
                    return 6;
                default:
                    return 6;
            }
        }

        void OnTick(object sender, EventArgs e)
        {
            if(aikanyt != DateTime.Now.Second)
            {
                PullMeOverMain.SetStreet(false);
                aikanyt = DateTime.Now.Second;
                if ((PullMeOverMain.state != PullMeOverMain.CopStates.Ticket || PullMeOverMain.state != PullMeOverMain.CopStates.StepOut) 
                    && PullMeOverMain.scriptenabled && Game.Player.WantedLevel == 0 && Game.Player.Character.IsInVehicle())
                {
                    PullMeOverMain.FindCops();
                }
                    
            }

            mymenupool.ProcessMenus();

            if (!Game.Player.Character.IsInVehicle() || !showdisplay || (!PullMeOverMain.scriptenabled && hidewhenscriptdisabled))
                return;

            CanDrawDisplay();
            speed = Game.Player.Character.CurrentVehicle.Speed;
            speedlimit = PullMeOverMain.speedlimit;
            speedunit = PullMeOverMain.SpeedConversion(speed);
            speedlimitunit = PullMeOverMain.SpeedConversion(speedlimit);
            SetTextColor();
            ShowSpeedDisplay();
        }

        bool PullOverAbleVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                playervehicle = Game.Player.Character.CurrentVehicle;
                if (playervehicle.Model.IsBicycle || playervehicle.Model.IsBike ||playervehicle.Model.IsCar || playervehicle.Model.IsQuadbike)
                    return true;
            }
            return false;
        }

        void CanDrawDisplay()
        {
            PullMeOverMain.pulloverable = PullOverAbleVehicle();
            showlimit = PullOverAbleVehicle();
        }

        void ShowSpeedDisplay()
        {     
                  
            speedText = new UIText(/*r+"r "+g+"g "+b+"b "+ */GetSpeedDisp() + GetSpeedLimit() + GetSpeedUnit(), point1, scale, textcolor, (GTA.Font.ChaletComprimeCologne), true, true, true);
            speedText.Draw();
        }

        string GetSpeedDisp()
        {
            if (showspeed && showlimit)
                return speedunit + " / ";
            else if (showspeed && !showlimit)
                return speedunit + "";
            return "    ";
        }

        string GetSpeedUnit()
        {
            if (showunit)
                return " " + PullMeOverMain.unit;
            return "    ";
        }

        string GetSpeedLimit()
        {
            if (showlimit)
                return "" + speedlimitunit;
            return "";
        }

        void SetTextColor()
        {
            textcolor = Color.FromArgb(255, r, g, b);
            /*  if (speed < speedlimit-4)
              {
                  textcolor = Color.FromArgb(200, 200, 200);
              }
              else
              {*/
            /*   r = (int)((speed / speedlimit) * 255);
             //  b = (int)((speed / speedlimit+wlimit) * 255);
               if (r > 255)
               {
                   g = 255 + (255-r);
                   r = 255;
                   if(g < 0)
                   {
                       b = -g;
                       g = 0;
                       if (b > 255)
                           b = 255;
                   }

               //0,255,0
               //255,255,0
               //255,0,0
               //255,0,255

               // 1 = 255 
               textcolor = Color.FromArgb(255, r, g, b);
                }

               /*  if (speed <= speedlimit)
                 {
                     textcolor = Color.LightGray;
                 }

                 else if (speed > speedlimit && speed < speedlimit + 4)
                 {
                     textcolor = Color.Yellow;
                 }

                 else if (speed > speedlimit + 4 && speed < wlimit + speedlimit)
                 {
                     textcolor = Color.OrangeRed;
                 }
                 else if (speed > wlimit + speedlimit)
                     textcolor = Color.FromArgb(255, 0, 255);*/

            /*  p = (int)(speed / speedlimit) * 255;

              if (r > 255)
              {
                  g = 255 + (255 - r);
                  r = 255;
                  if (g < 0)
                  {
                      b = -g;
                      g = 0;
                      if (b > 255)
                          b = 255;
                  }

              }

              if (speed <= speedlimit-4)
              {
                  //0,255,0
                  textcolor = Color.FromArgb(0, 255, 0);
              }

              else if (speed > speedlimit-4 && speed <= speedlimit + 4)
              {
                  //255,255,0
                  float vali = (speedlimit + 4) - (speedlimit - 4);
                  vali = (int)(vali/speed) * 255;
                  textcolor = Color.FromArgb(0, 255, 0);
              }

              else if (speed > speedlimit + 4 && speed < wlimit + speedlimit)
              {
                  //255,0,0
                  textcolor = Color.OrangeRed;
              }
              else if (speed > wlimit + speedlimit)
              {
                  //255,0,255
                  textcolor = Color.FromArgb(255, 0, 255);
              }*/


        }
    }
}
