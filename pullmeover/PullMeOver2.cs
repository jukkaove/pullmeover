using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;

using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace PullMeOver
{
    public partial class PullMeOverMain : Script
    {
        internal enum CopStates { None, Chase, Follow, Ticket, StepOut };
        internal static CopStates state;
        enum EventState { Locate, None };
        EventState eventstate = EventState.None;

        static float finalplayerspeed;
        internal static float speedlimit;
        internal static float wlimit;
        float stopsignspeed = 100;
        float speedingdifference = 0;
        static float speedlimitatcaught;
        static float vehiclelength;

        internal static bool scriptenabled;
        bool stoppedforcarfix;
        bool redlight;
        bool blips;
        bool enterehjadriverside;
        bool enterehjapassengerside;
        bool jacking;
        bool break_in;
        bool stoppedforhelmet;
        bool drivingstolenvehicle;
        internal static bool settingstolenvehicle;
        internal static bool settingnotifystolen;
        internal static bool settingwithouthelmet;
        internal static bool settingwheelie;
        internal static bool settingagainsttraffic;
        internal static bool settingcolliding;
        internal static bool settingnotrw;
        internal static bool settingmobilephone;
        internal static bool settingtailgating;
        internal static bool settingdrifting;
        internal static bool settingonsidewalk;
        internal static bool settingcanbereported;
        internal static bool settingstopsign;
        internal static bool settingcopcarchange;
        internal static bool settinglicensesuspension;
        internal static bool settingburnout;
        internal static bool settingwitnessblips;
        internal static bool settingapproachwithlights;
        internal static bool settinglanguage;
        internal static bool pulloverable;
        bool wasblipped;
        bool waschanged;
        static bool licensesuspended;
        bool debugss;
        bool debugv;

        Vector3 finalstop;
        Vector3 prevpos;

        Vehicle[] vehs;
        Vehicle copveh;
        internal static Vehicle playerVehicle;
        Vehicle enterVehicle;
        Vehicle jackedvehicle;
        public static Vehicle fleevehicle;
        
        Random rand = new Random();
        
        int ticks;
        int againsttrafficticks = 0;
        int tailgatingticks = 0;
        int slidingticks = 0;
        public static int range;
        internal static int hwaylimit;
        internal static int generallimit;
        int wantedspeedlimit;
        int burnoutticks = 0;
        int ticktimer = 0;
        int wheelieticks = 0;
        static int settingmaxviolations;
        int settingexpireminutes;
        int aikanyt = DateTime.Now.Second;
        internal static int[] addonpeds = { 2119533924, 1388307514, -1391788292, -1713450450, -560354151, -482908958, 1481343398, 640852342, 2017510805, -1883678939 };
        int[] trafficlights = { 1043035044, 865627822, 862871082, -655644382 };
        int language = 0;

        static string infractionlocation;
        static string infractionlocation2;
        internal static string unit;

        string[] ticketyells = { "CHALLENGE_THREATEN", "CHAT_STATE", /*"CRIMINAL_WARNING",*/ "PROVOKE_STARING", "WON_DISPUTE", "GENERIC_INSULT_MED" };

        Ped copped;
        Ped newcop;
        Ped jackeddriver;
        internal static Ped[] copsi;

        ScriptSettings config;
        internal static Keys enablescript;

        Blip copblip;

        Prop stopsign;

        Dictionary<int, bool> stolenlist;

        List<string> offences = new List<string>();
        List<Vehicle> autolist = new List<Vehicle>();
        static List<Record> recordlist = new List<Record>();
        List<Vehicle> seizedvehicles = new List<Vehicle>();
        List<Vehicle> evasionlist = new List<Vehicle>();
        static List<Record> felonylist = new List<Record>();
        public static List<BlipHandler> bliplist = new List<BlipHandler>();
        List<WitnessHandler> witnesslist = new List<WitnessHandler>();
        static List<CopsSentHandler> backuplist = new List<CopsSentHandler>();
        static List<Ped> backuppeds = new List<Ped>();

        internal static List<Ped> coplist = new List<Ped>();
        float sp1 = 0;

        public PullMeOverMain()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            //KeyUp += OnKeyUp;
            Aborted += OnAborted;
            Interval = 5;
            ticks = 0;
           // violationchecktime = DateTime.Now.AddMilliseconds(500);
            config = ScriptSettings.Load("scripts/PullMeOver.ini");
            scriptenabled = config.GetValue<bool>("Settings", "Enabled", true);
            enablescript = config.GetValue<Keys>("Settings", "Key", Keys.F11);
            //enablescriptmodifier = config.GetValue<Keys>("Settings", "Modifier key", Keys.Shift);
            wantedspeedlimit = config.GetValue<int>("Settings", "Exceed speed limit by", 50);
            range = config.GetValue<int>("Settings", "Range", 75);
            redlight = config.GetValue<bool>("Violations", "Running a red light", true);
            blips = config.GetValue<bool>("Settings", "Police car blips", true);
            settingstolenvehicle = config.GetValue<bool>("Crimes", "Stolen vehicles can be reported", false);
            settingnotifystolen = config.GetValue<bool>("Crimes", "Stolen vehicle notifications", false);
            settingwithouthelmet = config.GetValue<bool>("Violations", "Without helmet", true);
            settingwheelie = config.GetValue<bool>("Violations", "Wheelie/Stoppie", true);
            settingagainsttraffic = config.GetValue<bool>("Violations", "Driving against traffic", true);
            settingcolliding = config.GetValue<bool>("Violations", "Colliding ", true);
            settingnotrw = config.GetValue<bool>("Violations", "Heavily damaged vehicle", true);
            settingmobilephone = config.GetValue<bool>("Violations", "Using a mobile phone", true);
            settingtailgating = config.GetValue<bool>("Violations", "Tailgating ", true);
            settingdrifting = config.GetValue<bool>("Violations", "Drifting", true);
            settingonsidewalk = config.GetValue<bool>("Violations", "Driving on sidewalk", true);
            settingcanbereported = config.GetValue<bool>("Settings", "Can be reported", true);
            settingmaxviolations = config.GetValue<int>("Settings", "Max", 7);
            settingexpireminutes = config.GetValue<int>("Settings", "Minutes", 15);
            settingstopsign = config.GetValue<bool>("Violations", "Stop sign", true);
            settingburnout = config.GetValue<bool>("Violations", "Burnout/Excess revving", true);
            settingcopcarchange = config.GetValue<bool>("Settings", "Allow changing pursuing cop car", true);
            settinglicensesuspension = config.GetValue<bool>("Settings", "License suspension", true);
            settingwitnessblips = config.GetValue<bool>("Crimes", "Witness blips", false);
            settingapproachwithlights = config.GetValue<bool>("Settings", "Lights on when near", false);
            settinglanguage = config.GetValue<bool>("Settings", "Detect language", false);
            debugss = config.GetValue<bool>("Debug", "Debugss", false);
            debugv = config.GetValue<bool>("Debug", "Debugv", false);
            stolenlist = new Dictionary<int, bool>();
            SetSpeedLimits();

            if (settinglanguage)
                language = Function.Call<int>(Hash._GET_UI_LANGUAGE_ID);
            else
                language = 0;

            string str = config.GetValue("Settings", "Unit", "mph");
            if (str.ToUpper().Equals("KMH")){
                unit = "KMH";
                wlimit = wantedspeedlimit / 3.6f;
            }
            else
            {
                unit = "MPH";
                wlimit = wantedspeedlimit / 2.236936f;
            }
        }

        void OnTick(object sender, EventArgs e)
        {
            ticktimer++;
            
            TrackPlayer();
            Tickers();
            
        }

        internal static int SpeedConversion(float speed)
        {
            if (unit.Equals("KMH"))
                return (int)(speed * 3.6f);
            else
                return (int)(speed * 2.236936f);
        }
        internal static float SpeedMs(float speed)
        {
            if (unit.Equals("KMH"))
                return speed / 3.6f;
            else
                return speed / 2.236936f;
        }

        void Tickers()
        {    
            if (aikanyt != DateTime.Now.Second)
            {
                aikanyt = DateTime.Now.Second;

                int lkm = recordlist.Count;
                int flkm = felonylist.Count;

                for (int i = 0; i < bliplist.Count; i++)
                {
                    if (bliplist[i].IsVoided())
                        bliplist.Remove(bliplist[i]);
                }               
                for (int i = 0; i < recordlist.Count; i++)
                {
                    if (recordlist[i].Rauennut())
                        recordlist.Remove(recordlist[i]);
                }
                for (int i = 0; i < felonylist.Count; i++)
                {
                    if (felonylist[i].Rauennut())
                        felonylist.Remove(felonylist[i]);
                }
                
                if (felonylist.Count == 0 && licensesuspended && recordlist.Count < settingmaxviolations)
                {
                    for (int i = 0; i < seizedvehicles.Count; i++)
                    {
                        seizedvehicles[i].LockStatus = VehicleLockStatus.Unlocked;
                    }

                    UI.Notify("Your license was reinstated");
                    UI.Notify("~g~You may now use your vehicles");
                    licensesuspended = false;
                }

                for (int i = 0; i < witnesslist.Count; i++)
                {
                    if (witnesslist[i].ped.IsDead)
                        witnesslist[i].ped.CurrentBlip.Remove();

                    if (witnesslist[i].IsVoided())
                    {
                        if (witnesslist[i].ped.IsAlive)
                        {
                            int hashi = witnesslist[i].autohash;
                            if (stolenlist.Keys.Contains(hashi))
                            {
                                stolenlist[hashi] = true;
                                if (Game.Player.Character.IsInVehicle())
                                {
                                    if (playerVehicle.GetHashCode() == hashi)
                                        UI.Notify("~r~This vehicle has been reported stolen");
                                    drivingstolenvehicle = true;
                                }

                            }
                        }
                        witnesslist[i].ped.IsPersistent = false;
                    }
                }
                
            }
            
        }

        void OnAborted(object sender, EventArgs e)
        {
            for (int i = 0; i < seizedvehicles.Count; i++)
            {
                seizedvehicles[i].LockStatus = VehicleLockStatus.Unlocked;
                if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, seizedvehicles[i]))
                {
                    OutputArgument trailer = new OutputArgument();
                    Function.Call<Entity>(Hash.GET_VEHICLE_TRAILER_VEHICLE, seizedvehicles[i], trailer);
                    Entity temp = trailer.GetResult<Entity>();

                    if (!temp.Equals(null))
                    {
                        temp.IsPersistent = false;
                    }
                }
                seizedvehicles[i].IsPersistent = false;
            }
            for (int i = 0; i < bliplist.Count; i++) { bliplist[i].Void(); }
            EndEvent(false);
        }

        internal static void ShowRecord()
        {
            if (recordlist.Count > 0)
                UI.Notify("~y~" + recordlist.Count + "/" + settingmaxviolations + " ~w~Violations");
            else
                UI.Notify(recordlist.Count + "/" + settingmaxviolations + " Violations");
            if (felonylist.Count > 1)
                UI.Notify("~r~" + felonylist.Count + " ~w~Felonies");
            else if (felonylist.Count == 1)
                UI.Notify("~r~" + felonylist.Count + " ~w~Felony");
            else
                UI.Notify("~w~" + felonylist.Count + " Felonies");
            if (licensesuspended)
                UI.Notify("~r~License is suspended");
            else
                UI.Notify("~g~License is valid");
        }
        void OnKeyDown(object sender, KeyEventArgs e)
        {
            
            /*if (e.KeyCode == enablescript && !keyupped)
            {
                /*keycount++;
                if (keycount >= 2)
                {
                    keyupped = true;
                    scriptenabled = !scriptenabled;
                    if (scriptenabled)
                        UI.Notify("PullMeOver ~g~ENABLED");
                    else
                        UI.Notify("PullMeOver ~r~DISABLED");
                    keycount = 0;
                }
            }
            if (e.Shift && e.KeyCode == enablescript)
            {
                
            }*/
        }

        void SetChase()
        {
            copveh = copped.CurrentVehicle;
            state = CopStates.Chase;
            copped.IsPersistent = true;
            if(copveh == null)
                UI.ShowSubtitle("copve nulli");
            if(copped == null)
                UI.ShowSubtitle("copped nulli");
            if (blips)
            {
                Blip temppi = copveh.CurrentBlip;
                if (temppi != null && !copveh.CurrentBlip.Exists())
                    copveh.AddBlip();

                copblip = copveh.CurrentBlip;
                copblip.Sprite = BlipSprite.PoliceCarDot;
                copblip.Scale = 0.5f;
            }

            copped.BlockPermanentEvents = true;
            Action();
        }

        bool ChaserCanSee()
        {
            if (Vector3.Distance(copped.Position, Game.Player.Character.Position) < range)
            {
                if (Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, copped.Handle, Game.Player.Character.Handle, 17))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        bool IsInterrupted()
        {
            if (!copped.Exists())
            {
                EndEvent(false);
                return true;
            }
            if (copped.IsDead || Game.Player.Character.IsDead)
            {
                EndEvent(false);
                return true;
            }
            if (Game.Player.WantedLevel > 0)
            {
                if (copped.IsAlive && !copped.IsInVehicle())
                {
                    copped.Task.ClearAllImmediately();
                    copped.BlockPermanentEvents = false;
                    copped.Task.FightAgainst(Game.Player.Character, 2000);
                }
                EndEvent(false);
                return true;
            }

            if (state == CopStates.StepOut)
            {
                if (Game.Player.Character.IsInVehicle() && Vector3.Distance(copped.Position, Game.Player.Character.Position) > 15)
                {
                    Game.Player.WantedLevel = 1;
                    EndEvent(false);
                    return true;
                }
                if (!Game.Player.Character.IsInVehicle())
                {
                    playerVehicle.IsPersistent = true;

                    if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, playerVehicle))
                    {
                        OutputArgument trailer = new OutputArgument();
                        Function.Call<Entity>(Hash.GET_VEHICLE_TRAILER_VEHICLE, playerVehicle, trailer);
                        Entity temp = trailer.GetResult<Entity>();

                        if (!temp.Equals(null))
                            temp.IsPersistent = true;
                    }

                    playerVehicle.LockStatus = VehicleLockStatus.LockedForPlayer;
                    playerVehicle.EngineRunning = false;
                    seizedvehicles.Add(playerVehicle);
                    return true;
                }
                return false;
            }
            if (state == CopStates.Chase || state == CopStates.Follow)
            {
                if (!Game.Player.Character.IsInVehicle())
                { 
                    if (Vector3.Distance(copped.Position, Game.Player.Character.Position) <= 350)
                    {
                        if (!Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, playerVehicle.Handle, copped.Handle, 17))
                        {
                            eventstate = EventState.Locate;
                            return true;
                        }
                        else
                        {
                            Game.Player.WantedLevel = 1;
                            EndEvent(false);
                            return true;
                        }
                    }
                }
            }

            if (state == CopStates.Ticket)
            {
                if (Game.Player.Character.IsInVehicle())
                {
                    if (Vector3.Distance(finalstop, Game.Player.Character.Position) > 5)
                    {
                        copped.Task.ClearAllImmediately();
                        copped.BlockPermanentEvents = false;
                        copped.Task.EnterVehicle(copveh, VehicleSeat.Driver);
                        Game.Player.WantedLevel = 1;
                        EndEvent(false);
                        return true;
                    }
                }
                else if (!Game.Player.Character.IsInVehicle())
                {
                    copped.Task.ClearAllImmediately();
                    //   copped.Task.EnterVehicle(copveh, VehicleSeat.Driver);
                    copped.BlockPermanentEvents = false;
                    copped.Task.Arrest(Game.Player.Character);//.FightAgainst(Game.Player.Character, 2000);
                    Game.Player.WantedLevel = 1;
                    EndEvent(false);
                    return true;
                }
            }

            if (state != CopStates.Ticket)
                TrackPlayer();

            return false;
        }

        void ExcessSpeeding(int rangex, int cticks)
        {
            for (int i = 0; i < coplist.Count; i++)
            {
                if (!Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, coplist[i].Handle, Game.Player.Character.Handle, 17))
                    return;

                if ((playerVehicle.Speed - speedlimit) >= wlimit)
                    Game.Player.WantedLevel = 1;                 
            }    
        } 

        Vector3 PlayaPosition()
        {
            Vector3 pposi;
            Vehicle vehi = playerVehicle;
            Game.Player.Character.Task.WarpOutOfVehicle(vehi);
            Wait(0);
            pposi = Game.Player.Character.Position;
            Game.Player.Character.Task.WarpIntoVehicle(vehi, VehicleSeat.Driver);
            return pposi;
        }

        void Arrest()
        {
            finalstop = Game.Player.Character.Position;
            // SendCops();
            if (OdotusLooppi(2))
                return;

            copveh.LockStatus = VehicleLockStatus.Locked;
            copped.Task.LeaveVehicle(copveh, false);

            copped.Task.ClearAll();
            copped.Weapons.Give(WeaponHash.StunGun, 1, false, true);
            copped.Weapons.Give(WeaponHash.Pistol, 120, true, true);

            World.SetRelationshipBetweenGroups(Relationship.Hate, copped.RelationshipGroup, Game.Player.Character.RelationshipGroup);
            World.SetRelationshipBetweenGroups(Relationship.Hate, Game.Player.Character.RelationshipGroup, copped.RelationshipGroup);

            if (OdotusLooppi(1))
                return;
            copped.Task.AimAt(Game.Player.Character, -1);
            Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "CHALLENGE_THREATEN", "SPEECH_PARAMS_SHOUTED_CLEAR");

            UI.ShowSubtitle("~o~Do not move!", 8000);
            if (OdotusLooppi(10))
                return;

            Vector3 playpos = StoppedPlayerPostion();
            Function.Call(Hash.TASK_GOTO_ENTITY_AIMING, copped, Game.Player.Character.Handle, 1.5f, 30f);
            ticks = 0;
            while (!IsInterrupted())
            {

                float dist = Vector3.Distance(playpos, copped.Position);
                float dist2 = Vector3.Distance(Game.Player.Character.Position, finalstop);
                ticks++;

                if (dist2 > 5 || !Game.Player.Character.IsInVehicle())
                {
                    copped.Task.ClearAll();
                    Game.Player.WantedLevel = 1;
                    break;
                }

                if (dist < 1f)
                {
                    break;
                }
                else if (dist < 2.5f && ticks > 1500)
                    break;

                if (ticks > 3000)
                    break;

                Wait(0);
            }

            copped.Weapons.Give(WeaponHash.StunGun, 1, true, true);
            Function.Call(Hash.TASK_ARREST_PED, copped, Game.Player.Character);
            if (OdotusLooppi(4))
                return;
            EndEvent(false);
        }

        Vector3 StoppedPlayerPostion()
        {
            if (playerVehicle.Model.IsCar)
            {
                if (playerVehicle.IsDoorBroken(VehicleDoor.FrontLeftDoor))
                    return PlayaPosition();
                else if (playerVehicle.HasBone("seat_dside_f") && playerVehicle.HasBone("seat_pside_f"))
                {
                    Vector3 vector = playerVehicle.GetBoneCoord("seat_dside_f") - playerVehicle.GetBoneCoord("seat_pside_f");
                    return playerVehicle.GetBoneCoord("seat_dside_f") + vector;
                }

                else
                    return PlayaPosition();
            }
            else if (playerVehicle.Model.IsBicycle || playerVehicle.Model.IsBike)
            {
                Vector3 pos4 = Vector3.Cross(Game.Player.Character.ForwardVector, -Game.Player.Character.UpVector);
                return playerVehicle.Position + pos4;
            }
            else
                return PlayaPosition();
        }

        void TicketTime(Vehicle copveh, Ped copped)
        {

            state = CopStates.Ticket;
            if (offences.Contains("stolen"))
            {
                Arrest();
                return;
            }

            copveh.LockStatus = VehicleLockStatus.Locked;
            copped.Task.LeaveVehicle(copveh, true);

            finalstop = Game.Player.Character.Position;

            float dist = Vector3.Distance(Game.Player.Character.Position, copped.Position);
            float dist2;
            Vector3 playpos = StoppedPlayerPostion();

            playpos += Game.Player.Character.ForwardVector*0.2f;

            Wait(1);
            copped.Task.GoTo(playpos, false, -1);
            ticks = 0;
            while (!IsInterrupted())
            {
                //Function.Call(Hash.DRAW_LINE, copped.Position.X, copped.Position.Y, copped.Position.Z, playpos.X, playpos.Y, playpos.Z, 255, 0, 0, 255);
                dist = Vector3.Distance(playpos, copped.Position);
                dist2 = Vector3.Distance(Game.Player.Character.Position, finalstop);
                ticks++;

                if (dist2 > 5 || !Game.Player.Character.IsInVehicle())
                {
                    copped.Task.ClearAll();
                    Game.Player.WantedLevel = 1;
                    break;
                }

                if (dist < 1f || (dist < 2.5f && copped.Velocity == Vector3.Zero))
                    break;

                if (ticks > 3000)
                    break;
                Wait(0);
            }

            if (IsInterrupted()) return;
            if (ticks > 3000)
            {
                EndEvent(false);
                UI.Notify("~r~Pullover aborted");
                return;
            }

            copped.Task.TurnTo(Game.Player.Character, 1500);
            Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "GENERIC_HI", "SPEECH_PARAMS_FORCE");
            while (Function.Call<bool>(Hash.IS_ANY_SPEECH_PLAYING, copped))
            {
                Wait(0);
            }

            if (OdotusLooppi(1))
                return;
            bool arrest = false;

            if (offences.Contains("sliding") && offences.Contains("burnout"))
                offences.Remove("burnout");

            int olkm = offences.Count;
            int rlkm = recordlist.Count;
            int flkm = felonylist.Count;
            bool isevasionvehicle = evasionlist.Contains(playerVehicle);
            if (!arrest)
            {
                Function.Call(Hash._PLAY_AMBIENT_SPEECH2, Game.Player.Character, "GENERIC_HOWS_IT_GOING", "SPEECH_PARAMS_STANDARD");

                if (OdotusLooppi(1))
                    return;

                Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "KIFFLOM_GREET", "SPEECH_PARAMS_FORCE");
                Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, copped, "CODE_HUMAN_MEDIC_TIME_OF_DEATH", copped.Position.X,
                    copped.Position.Y, copped.Position.Z, copped.Heading, offences.Count - 1 * 8000, false, false);

                if (OdotusLooppi(2))
                    return;

                for (int i = 0; i < offences.Count; i++)
                {
                    if (offences[i].Equals("speeding"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle("You were doing ~r~" + SpeedConversion(finalplayerspeed) + " ~w~in a " + SpeedConversion(speedlimitatcaught) +
                            " "+unit+" zone on ~g~" + infractionlocation + "~w~!", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("onpavement"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle("Your driving is endangering pedestrians!", 8000);
      
                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("againsttraffic"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" You were driving against traffic on ~g~" + infractionlocation2 + "~w~!", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("collision"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" That was quite a collision you had there.", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("notrw"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" Your vehicle is heavily damaged. Please get it fixed.", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("phone"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" What was that in your hands? A phone? You know that's a traffic violation", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("tailgating"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" You were driving really close to other vehicle", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("sliding"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" You seem to have trouble controlling your vehicle", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("redlight"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" You ran a red light!", 8000);

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("helmet"))
                    {
                        if (language != 7)
                        {
                            if (Game.Player.Character.IsWearingHelmet)
                                UI.ShowSubtitle(" Oh, now you found your helmet", 8000);
                            else
                                UI.ShowSubtitle(" Where is your helmet?!", 8000);
                        }

                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("wheelie"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" This is not a stunt track!", 8000);
                  
                        Spiikki("ticket");
                    }
                    else if (offences[i].Equals("stopsign"))
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" You didn't come to a stop at a stop sign", 8000);

                        Spiikki("ticket");
                    }
                    /* else if (offences[i].Equals("burnout"))
                     {
                         if (language != 7)
                             UI.ShowSubtitle(" We get it, you have a powerful engine", 8000);

                         Spiikki("ticket");
                     }*/

                    if (OdotusLooppi(6))
                        break;

                    if (i != offences.Count - 1 || isevasionvehicle || recordlist.Count >= settingmaxviolations || felonylist.Count > 0)
                    {
                        if (language != 7)
                            UI.ShowSubtitle(" Also...", 8000);

                        if (OdotusLooppi(4))
                            break;
                    }
                }
            }


            if (isevasionvehicle)
            {
                if (language != 7)
                    UI.ShowSubtitle(" ~r~You shouldn't run from the police. This vehicle was used in a felony evasion", 8000);

                evasionlist.Remove(playerVehicle);
                if (OdotusLooppi(6))
                    return;
            }
            else if (recordlist.Count >= settingmaxviolations || felonylist.Count > 0)
            {
                if (language != 7)
                    UI.ShowSubtitle(" ~r~You are driving on suspended license", 8000);

                if (OdotusLooppi(6))
                    return;
            }

            LisaaRecordiin(olkm);
            if ((isevasionvehicle && settinglicensesuspension) || (licensesuspended && settinglicensesuspension))
            {
                felonylist.Add(new Record(DateTime.Now.AddMinutes(settingexpireminutes)));
                licensesuspended = true;
            }
            else if (recordlist.Count >= settingmaxviolations && settinglicensesuspension)
            {
                licensesuspended = true;
            }
            ticks = 0;
            /* if (arrest)
             {
                 
                 EndEvent();
             }*/

            if (Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, copped))
            {
                copped.Task.ClearAll();
                if (OdotusLooppi(3))
                    return;
                if ((recordlist.Count >= settingmaxviolations && settinglicensesuspension) || (felonylist.Count > 0 && settinglicensesuspension))
                {
                    if (language != 7)
                        UI.ShowSubtitle(" ~o~Hold on...", 8000);

                }

                if (OdotusLooppi(4))
                    return;
            }

            if (IsInterrupted()) { EndEvent(false); return; }

            if (!licensesuspended)
            {
                Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "CRIMINAL_WARNING", "SPEECH_PARAMS_FORCE");
                Wait(200);
                if (!Function.Call<bool>(Hash.IS_AMBIENT_SPEECH_PLAYING, copped))
                    Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "GENERIC_BYE", "SPEECH_PARAMS_FORCE");
            }

            for (int i = 0; i < offences.Count; i++)
            {
                if (offences[i].Equals("speeding"))
                {
                    int limit = SpeedConversion(speedlimitatcaught);
                    int pspeed = SpeedConversion(finalplayerspeed);


                    UI.Notify("You were fined ~r~$" + (pspeed - limit) * 20 + "~w~ for going " + (pspeed - limit) + " over the speed limit.");
                    Game.Player.Money = Game.Player.Money - (pspeed - limit) * 20;
                }
                else if (offences[i].Equals("onpavement"))
                {
                    UI.Notify("You were fined ~r~$350 ~w~for unsafe driving.");
                    Game.Player.Money = Game.Player.Money - 350;
                }
                else if (offences[i].Equals("againsttraffic"))
                {
                    UI.Notify("You were fined ~r~$250 ~w~for careless driving.");
                    Game.Player.Money = Game.Player.Money - 250;
                }
                else if (offences[i].Equals("collision"))
                {
                    UI.Notify("You were fined ~r~$500 ~w~for damage to property.");
                    Game.Player.Money = Game.Player.Money - 500;
                }
                else if (offences[i].Equals("notrw"))
                {
                    UI.Notify("You were fined ~r~$180 ~w~for unroadworthy vehicle.");
                    Game.Player.Money = Game.Player.Money - 180;
                    stoppedforcarfix = true;
                }
                else if (offences[i].Equals("phone"))
                {
                    UI.Notify("You were fined ~r~$100 ~w~for using a mobile phone while driving.");
                    Game.Player.Money = Game.Player.Money - 100;
                }
                else if (offences[i].Equals("tailgating"))
                {
                    UI.Notify("You were fined ~r~$100 ~w~for tailgating.");
                    Game.Player.Money = Game.Player.Money - 100;
                }
                else if (offences[i].Equals("sliding"))
                {
                    UI.Notify("You were fined ~r~$350 ~w~for reckless driving.");
                    Game.Player.Money = Game.Player.Money - 350;
                }
                else if (offences[i].Equals("redlight"))
                {
                    UI.Notify("You were fined ~r~$450 ~w~for running a red light.");
                    Game.Player.Money = Game.Player.Money - 450;
                }
                else if (offences[i].Equals("helmet"))
                {
                    UI.Notify("You were fined ~r~$200 ~w~for not wearing motorcycle helmet.");
                    Game.Player.Money = Game.Player.Money - 200;
                    stoppedforhelmet = true;
                }
                else if (offences[i].Equals("wheelie"))
                {
                    UI.Notify("You were fined ~r~$350 ~w~for reckless driving.");
                    Game.Player.Money = Game.Player.Money - 350;
                }
                else if (offences[i].Equals("stopsign"))
                {
                    UI.Notify("You were fined ~r~$240 ~w~for a failure to stop at a stop sign.");
                    Game.Player.Money = Game.Player.Money - 240;
                }
                else if (offences[i].Equals("burnout"))
                {
                    UI.Notify("You were fined ~r~$140 ~w~for exhibition of power.");
                    Game.Player.Money = Game.Player.Money - 140;
                }
            }

            if ((recordlist.Count >= settingmaxviolations && settinglicensesuspension) || (felonylist.Count > 0 && settinglicensesuspension))
            {
                if (recordlist.Count >= settingmaxviolations)
                    UI.Notify("You have ~y~" + recordlist.Count + "/" + settingmaxviolations + " ~w~violations on your record");
                state = CopStates.StepOut;
                GetOut(playpos, olkm, flkm, rlkm);
            }
            else
                UI.Notify("You have ~y~" + recordlist.Count + "/" + settingmaxviolations + " ~w~violations on your record");

            if (!Game.Player.Character.IsInVehicle() && Game.Player.WantedLevel == 0 && state == CopStates.StepOut)
            {
                if (language != 7)
                    UI.ShowSubtitle(" Your vehicle has been seized. You are not allowed to drive", 6000);
                else
                {
                    UI.ShowSubtitle("Ваше транспортное средство было изъято", 3000);
                    if (OdotusLooppi(3))
                        return;
                    UI.ShowSubtitle("Вам запрещено управлять данным ТС", 3000);
                }
                Wait(2000);
            }

            Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "CRIMINAL_WARNING", "SPEECH_PARAMS_FORCE");
            Wait(200);
            if (!Function.Call<bool>(Hash.IS_AMBIENT_SPEECH_PLAYING, copped))
                Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "GENERIC_BYE", "SPEECH_PARAMS_FORCE");

            copped.Task.EnterVehicle(copveh, VehicleSeat.Driver);
            copped.BlockPermanentEvents = false;
            Wait(1000);
            EndEvent(false);

        }

  /*      void Tekstitys(int id, int ms)
        {
            if(language != 7)
                UI.ShowSubtitle(eng[id], ms);
            else
            {
                switch (id)
                {
                    case 4:
                        UI.ShowSubtitle("Ваше авто имеет сильные повреждения", 4000);
                        if (OdotusLooppi(3))
                            return;
                        UI.ShowSubtitle("Пожалуйста, почините его.", 4000);
                        break;
                }
            }
        }*/

        void GetOut(Vector3 playpos, int olkm, int flkm, int rlkm)
        {
            Vector3 newplaypos = playpos + Game.Player.Character.ForwardVector * -3;
            copped.Task.GoTo(newplaypos, false, 5000);
            int loops = 0;
            while(!IsInterrupted())
            {
                loops++;
                if (Vector3.Distance(newplaypos, copped.Position) < 1 || (copped.Velocity == Vector3.Zero && loops > 60))
                    break;

                Wait(100);
            }
            copped.Task.ClearAll();
            copped.Task.TurnTo(Game.Player.Character, -1);
            Wait(1500);
            Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, copped, "WORLD_HUMAN_COP_IDLES", 0, 1);
            Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "GENERIC_INSULT_HIGH", "SPEECH_PARAMS_FORCE");

            if ((rlkm < settingmaxviolations) && (recordlist.Count >= settingmaxviolations) && (flkm == 0))
            {
                if (language != 7)
                    UI.ShowSubtitle(" ~y~You have too many violations on record.\n~w~ Your license is now suspended. Please step out of the vehicle", 8000);
                else
                {
                    UI.ShowSubtitle("~o~У вас слишком много нарушений ПДР", 4000);
                    if (OdotusLooppi(4))
                        return;
                    UI.ShowSubtitle("~o~Я вынужден анулировать вашу лицензию водителя", 4000);
                }
            }
            else
            {
                if (language != 7)
                    UI.ShowSubtitle("~o~ Your license is suspended.~w~\n Please step out of the vehicle", 8000);
                else
                {
                    UI.ShowSubtitle("~o~Ваша лицензия приостановлена", 4000);
                    if (OdotusLooppi(4))
                        return;
                    UI.ShowSubtitle("~o~Пожалуйста, выйдите из машины", 4000);
                }
            }

            while (!IsInterrupted())
            {
                Wait(0);
            }

        }

        void Spiikki(string keissi)
        {
            string randi = "";

            while (!Function.Call<bool>(Hash.IS_ANY_SPEECH_PLAYING, copped))
            {
                randi = ticketyells[rand.Next(ticketyells.Length)];
                Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, randi, "SPEECH_PARAMS_FORCE");
                Wait(0);
            }
        }

       /* void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == enablescript)
            {
                keyupped = false;
                keycount = 0;
            }
        }*/

        void EndEvent(bool change)
        {
            if (copped.Exists())
                if (copped.IsAlive)
                {
                    copped.Weapons.Give(WeaponHash.Pistol, 120, false, true);
                    copped.Weapons.Give(WeaponHash.StunGun, 1, false, true);
                    copped.Weapons.Give(WeaponHash.Nightstick, 1, false, true);
                    copped.BlockPermanentEvents = false;
                    copped.Task.ClearAll();
                }

            if (blips && copblip.Exists())
                copblip.Remove();

            if (Game.Player.WantedLevel > 0 && Game.Player.Character.IsInVehicle())
            {
                if (stolenlist.ContainsKey(Game.Player.Character.CurrentVehicle.GetHashCode()))
                    stolenlist.Remove(Game.Player.Character.CurrentVehicle.GetHashCode());
                drivingstolenvehicle = false;
            }
            
            Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, copveh, false);
            copveh.LockStatus = VehicleLockStatus.Unlocked;
            copped.IsPersistent = false;
            copveh.IsPersistent = false;
            copveh.MarkAsNoLongerNeeded();
            copped.MarkAsNoLongerNeeded();

            if (!change)
            {
                offences.Clear();
                state = CopStates.None;
                finalplayerspeed = 0;
                speedingdifference = 0;
                bliplist.Clear();
                wasblipped = false;
                waschanged = false;
            }
            else
                copped = newcop;

        }

        void TrackVehicle()
        {
            if (Game.Player.Character.IsJacking && settingstolenvehicle && !jacking && !Game.Player.Character.IsInVehicle())
            {
                jackeddriver = Game.Player.Character.GetJackTarget();
                jackedvehicle = jackeddriver.CurrentVehicle;

                if (jackedvehicle == null)
                    return;
                if (stolenlist.ContainsKey(jackedvehicle.GetHashCode()))
                    return;

                jacking = true;
                stolenlist.Add(jackedvehicle.GetHashCode(), false);

                Ped[] vehp = jackedvehicle.Passengers;

                if (settingwitnessblips)
                {
                    bliplist.Add(new BlipHandler(jackeddriver, DateTime.Now.AddSeconds(25), false));
                    for (int i = 0; i < vehp.Length; i++)
                    {
                        bliplist.Add(new BlipHandler(vehp[i], DateTime.Now.AddSeconds(25), false));
                    }
                }

                for (int i = 0; i < vehp.Length; i++)
                {
                    witnesslist.Add(new WitnessHandler(DateTime.Now.AddSeconds(25), vehp[i], jackedvehicle.GetHashCode()));
                    //   vehp[i].Task.LeaveVehicle(vehp[i].CurrentVehicle, false);
                    vehp[i].Task.ReactAndFlee(Game.Player.Character);
                }
                witnesslist.Add(new WitnessHandler(DateTime.Now.AddSeconds(25), jackeddriver, jackedvehicle.GetHashCode()));
                jacking = false;
                return;
            }

            if (seizedvehicles.Count == 0)
                return;
            enterVehicle = Game.Player.Character.GetVehicleIsTryingToEnter();
            if (enterVehicle.Exists())
            {
                int hashi = enterVehicle.GetHashCode();
                for (int i = 0; i < seizedvehicles.Count; i++)
                {
                    if (seizedvehicles[i].GetHashCode() == hashi && recordlist.Count < settingmaxviolations && felonylist.Count == 0 && !licensesuspended)
                    {
                        enterVehicle.LockStatus = VehicleLockStatus.Unlocked;
                        enterVehicle.IsPersistent = false;
                        if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, enterVehicle))
                        {
                            OutputArgument trailer = new OutputArgument();
                            Function.Call<Entity>(Hash.GET_VEHICLE_TRAILER_VEHICLE, enterVehicle, trailer);
                            Entity temp = trailer.GetResult<Entity>();

                            if (!temp.Equals(null))
                                temp.IsPersistent = false;
                        }
                        seizedvehicles.Remove(enterVehicle);
                        break;
                    }
                }
            }
            if (!settingstolenvehicle)
                return;
            if (!enterVehicle.Exists() && !break_in)
                return;
            if (enterVehicle.Model.IsCar && enterVehicle.IsSeatFree(VehicleSeat.Driver) && !break_in)
            {
                if (Function.Call<bool>(Hash.IS_VEHICLE_WINDOW_INTACT, enterVehicle, 0))
                {
                    break_in = true;
                    enterehjadriverside = true;
                }
                if (Function.Call<bool>(Hash.IS_VEHICLE_WINDOW_INTACT, enterVehicle, 1))
                {
                    break_in = true;
                    enterehjapassengerside = true;
                }

            }
            if (!break_in)
                return;
            if (Game.Player.Character.IsInVehicle() && break_in)
            {
                break_in = false;

                if (Function.Call<bool>(Hash.IS_VEHICLE_WINDOW_INTACT, Game.Player.Character.CurrentVehicle, 0) && enterehjadriverside)
                    enterehjadriverside = false;
                if (Function.Call<bool>(Hash.IS_VEHICLE_WINDOW_INTACT, Game.Player.Character.CurrentVehicle, 1) && enterehjapassengerside)
                    enterehjapassengerside = false;

                if (!enterehjapassengerside && !enterehjadriverside)
                    return;
                else
                {
                    enterehjadriverside = false;
                    enterehjapassengerside = false;
                    int radius = 30;
                    if (Game.Player.Character.CurrentVehicle.AlarmActive)
                        radius = 50;
                    Ped[] pedit = World.GetNearbyPeds(Game.Player.Character.Position, radius);

                    for (int i = 0; i < pedit.Length; i++)
                    {
                        if (pedit[i].IsHuman && !pedit[i].Equals(Game.Player.Character) && pedit[i].IsAlive)
                            if (Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, pedit[i].Handle, Game.Player.Character.Handle, 17))
                            {
                                if (pedit[i].IsOnFoot)
                                    pedit[i].Task.TurnTo(Game.Player.Character, 15000);

                                if (settingwitnessblips)
                                    bliplist.Add(new BlipHandler(pedit[i], DateTime.Now.AddSeconds(15), false));

                                witnesslist.Add(new WitnessHandler(DateTime.Now.AddSeconds(15), pedit[i], Game.Player.Character.CurrentVehicle.GetHashCode()));

                                if (!stolenlist.ContainsKey(Game.Player.Character.CurrentVehicle.GetHashCode()))
                                {
                                    if (settingnotifystolen)
                                        UI.Notify("~y~Vehicle theft was witnessed");

                                    stolenlist.Add(Game.Player.Character.CurrentVehicle.GetHashCode(), false);
                                }
                            }
                    }
                }
            }
        }

        void TrackPlayer()
        {
            if (state == CopStates.Ticket || state == CopStates.StepOut)
                return;

            if (scriptenabled)
                TrackVehicle();

           // if (Function.Call<int>(Hash.GET_PED_TYPE, Game.Player.Character) == 6)
          //      return;

            if (scriptenabled && Game.Player.WantedLevel == 0 /*&& violationchecktime < DateTime.Now*/)
            {
                //violationchecktime = DateTime.Now.AddMilliseconds(100);
                if (!Game.Player.Character.IsInVehicle())
                    return;
                
                if (Function.Call<bool>(Hash.IS_VEHICLE_SIREN_ON, playerVehicle))
                    return;

                if (!Game.Player.Character.CurrentVehicle.Equals(playerVehicle))
                {
                    playerVehicle = Game.Player.Character.CurrentVehicle;
                    vehiclelength = playerVehicle.Model.GetDimensions().Y;
                    Wait(1);
                    stoppedforcarfix = false;
                    stoppedforhelmet = false;

                    if (settingstolenvehicle)
                    {
                        if (stolenlist.ContainsKey(playerVehicle.GetHashCode()))
                        {
                            if (stolenlist[playerVehicle.GetHashCode()])
                            {
                                if (settingnotifystolen)
                                    UI.Notify("~r~This vehicle has been reported stolen");
                                drivingstolenvehicle = true;
                            }
                            else
                                drivingstolenvehicle = false;
                        }
                        else
                            drivingstolenvehicle = false;
                    }
                }

                if (!pulloverable)
                    return;
                if (playerVehicle.IsInAir)
                    return;

                if (!Function.Call<bool>(Hash._IS_VEHICLE_ENGINE_ON, playerVehicle))
                    return;
                

                if (drivingstolenvehicle)
                {
                    SetOffence("stolen", 30, true, true, false, true, false);
                }

                if (DrivingAgainstTraffic(Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_AGAINST_TRAFFIC, Game.Player)))
                {
                    SetOffence("againsttraffic", range, true, true, true, true, false);
                }
                Wait(1);
                if (NotRoadworthy())
                {
                    SetOffence("notrw", range, true, true, false, true, false);
                }
                sp1 = playerVehicle.Speed;
                Wait(100);
                // Wait(1);
                float sp2 = playerVehicle.Speed;            
              
                if (settingcolliding && (sp2 - sp1 < -5) || (playerVehicle.HasCollidedWithAnything && playerVehicle.Speed > 15))
                {
                    //UI.ShowSubtitle((sp2 - sp1) + "");
                    SetOffence("collision", range, true, true, true, true, false);
                }
                
              //  UI.ShowSubtitle(sp1 + "");
                Wait(1);
                if (UsingMobilePhone())
                {
                    SetOffence("phone", 25, true, true, false, true, false);
                }
                Wait(1);
                if (Tailgating())
                {
                    SetOffence("tailgating", range, true, true, false, true, false);
                }
                Wait(1);
                if (Sliding())
                {
                    SetOffence("sliding", range, true, true, true, true, false);
                }
                Wait(1);
                if (RunningRedLight())
                {
                    SetOffence("redlight", range, true, true, true, true, false);
                }
                Wait(1);
                if (WithoutHelmet())
                {
                    SetOffence("helmet", range, true, true, false, true, false);
                }
                Wait(1);
                if (Wheelie())
                {
                    SetOffence("wheelie", range, true, true, true, true, false);
                }
                Wait(1);
                if (DrivingOnPavement())
                {
                    SetOffence("onpavement", range, true, true, true, true, false);
                }
                Wait(1);
                if (StopSignViolation())
                {
                    SetOffence("stopsign", range, true, true, false, true, false);
                }
                Wait(1);
                if (playerVehicle.Speed > (speedlimit + 4))
                {
                    SetOffence("speeding", range, true, true, true, true, false);
                }
            }
        }

        void SetOffence(string offence, int rangex, bool cansee, bool driver, bool pedcop, bool setchase, bool changechaser)
        {
            if(!offences.Contains(offence) || offence.Equals("speeding"))
            {
                if (state == CopStates.None)
                {
                    if (GetCop(rangex, cansee, driver, pedcop, setchase, changechaser) != null)
                    {
                        if(offence.Equals("againsttraffic"))
                            infractionlocation2 = World.GetStreetName(Game.Player.Character.Position);

                        if (offence.Equals("speeding"))
                        {
                            speedingdifference = playerVehicle.Speed - speedlimit;
                            infractionlocation = World.GetStreetName(Game.Player.Character.Position);
                            speedlimitatcaught = speedlimit;
                            finalplayerspeed = playerVehicle.Speed;
                        }
                        AddOffence(offence);
                        SetChase();
                    }
                }
                else if (state == CopStates.Chase || state == CopStates.Follow)
                {
                    if (ChaserCanSee())
                    {
                        if (offence.Equals("againsttraffic"))
                            infractionlocation2 = World.GetStreetName(Game.Player.Character.Position);
                        if (offence.Equals("speeding") && (playerVehicle.Speed - speedlimit) > speedingdifference && ChaserCanSee())
                        {
                            infractionlocation = World.GetStreetName(Game.Player.Character.Position);
                            finalplayerspeed = playerVehicle.Speed;
                            speedingdifference = playerVehicle.Speed - speedlimit;
                            speedlimitatcaught = speedlimit;
                        }
                        AddOffence(offence);
                    }
                        
                }
            }
        }

        int SpeedLimitAtStreet()
        {
            if (freeways.Contains(infractionlocation))
                return hwaylimit;
            return generallimit;
        }

        void Clearplayer()
        {
            offences.Clear();
        }

        void LisaaRecordiin(int lkm)
        {
            for (int i = 0; i < lkm; i++)
            {
                recordlist.Add(new Record(DateTime.Now.AddMinutes(settingexpireminutes)));
            }
        }

        void AddOffence(string offence)
        {

            if (!offences.Contains(offence))
            {
                offences.Add(offence);
                if (debugv)
                    UI.Notify(offence);
            }

        }

        List<string> ReturnOffences()
        {
            return offences;
        }

        void RecordLoop()
        {
            for (int i = 0; i < recordlist.Count; i++)
            {
                recordlist.Remove(recordlist[i]);
            }
        }

        bool OdotusLooppi(int secs)
        {

            for (int i = 0; i < secs; i++)
            {
                if (!IsInterrupted())
                    Wait(1000);
                else
                    return true;
            }

            return false;
        }

        void MoreCops()
        {
            for (int i = 0; i < coplist.Count; i++)
            {
                if (backuppeds.Contains(coplist[i]))
                    continue;
                
                backuplist.Add(new CopsSentHandler(coplist[i]));
                backuppeds.Add(coplist[i]);
                if (coplist[i].IsInVehicle())
                {
                    Vehicle tempveh = coplist[i].CurrentVehicle;
                    if (tempveh.Model.IsCar || tempveh.Model.IsBike)
                    {
                        tempveh.SirenActive = true;
                    }
                }   
            }
        }

        void SendCops()
        {
            eventstate = EventState.None;
            waschanged = false;

            fleevehicle = playerVehicle;
            fleevehicle.IsPersistent = true;
            // DateTime endtime = DateTime.Now.AddSeconds(60);
            bool vehiclefound = false;
            int times = 0;
            while (Game.Player.WantedLevel == 0 && !Game.Player.Character.IsDead && times < 60)
            {
                if (Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, playerVehicle.Handle, copped.Handle, 17))
                {
                    if (Game.Player.Character.IsInVehicle())
                    {
                        if (Game.Player.Character.CurrentVehicle.Equals(fleevehicle))
                        {
                            waschanged = true;
                            break;
                        }

                    }
                    //  UI.Notify("~y~Your vehicle was found");
                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                    vehiclefound = true;
                    break;
                }
                Wait(500);
                times++;
            }
            if (waschanged)
                Action();
            
            if (!vehiclefound)
            {
                EndEvent(false);
                return;
            }

            Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, copveh, false);

            if (blips && copblip.Exists())
                copblip.Remove();
            backuplist.Add(new CopsSentHandler(copped));
          //  Ped[] copsi = World.GetNearbyPeds(playerVehicle.Position, 500);

            times = 0;


            while (times < 120)
            {
                //FindCops();
                if(times == 119)
                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                if (Game.Player.WantedLevel > 0 || Game.Player.Character.IsDead/* || DateTime.Now > endtime*/)
                    break;

                for (int i = 0; i < backuplist.Count; i++)
                {
                    backuplist[i].Check(false);
                }

                MoreCops();
                Wait(500);
                times++;
            }

            for (int i = 0; i < backuppeds.Count; i++)
            {
                if(backuppeds[i].Exists())
                    backuppeds[i].Task.ClearAll();
            }

            for (int i = 0; i < bliplist.Count; i++)
            {
                bliplist[i].Void();
            }

            fleevehicle.IsPersistent = false;
            fleevehicle = null;
            backuplist.Clear();
            backuppeds.Clear();
            EndEvent(false);
        }
    }

    class Record
    {
        DateTime old;

        public Record(DateTime old)
        {
            this.old = old;
        }

        public bool Rauennut()
        {
            int arresttime = Function.Call<int>(Hash.GET_TIME_SINCE_LAST_ARREST);
            int deathtime = Function.Call<int>(Hash.GET_TIME_SINCE_LAST_DEATH);

            if ((arresttime > 1000 && arresttime < 30000) || (deathtime > 1000 && deathtime < 30000) || (old < DateTime.Now))
                return true;
            else
                return false;
        }
    }

    class WitnessHandler
    {
        DateTime old { get; set; }
        public Ped ped { get; }
        public int autohash { get; }

        public WitnessHandler(DateTime old, Ped ped, int autohash)
        {
            this.old = old;
            this.ped = ped;
            ped.IsPersistent = true;
            this.autohash = autohash;
        }

        public bool IsVoided()
        {
            if ((old < DateTime.Now))
                return true;
            else
                return false;
        }
    }

    class CopsSentHandler
    {
        public Ped ped { get; set; }
        Vector3 nextpos;
        Random rand = new Random();

        public CopsSentHandler(Ped ped)
        {
            PullMeOverMain.bliplist.Add(new BlipHandler(ped, DateTime.Now.AddSeconds(60), true));
            this.ped = ped;
            SetPos();
        }

        void SetPos()
        {
            if (ped.IsInVehicle())
            {
                if (ped.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver).Equals(ped))
                {
                    if (PullMeOverMain.playerVehicle != null)
                        nextpos = PullMeOverMain.playerVehicle.Position;
                    else
                        nextpos = World.GetNextPositionOnStreet(Game.Player.Character.Position);
                    ped.Task.DriveTo(ped.CurrentVehicle, nextpos, 10, 50);
                }
            }
            else
            {
                nextpos = World.GetSafeCoordForPed(Game.Player.Character.Position, false);
                ped.Task.RunTo(nextpos);
            }
            float dist2 = Vector3.Distance(Game.Player.Character.Position, nextpos);
        }
        public void Check(bool end)
        {
            if (Vector3.Distance(PullMeOverMain.fleevehicle.Position, Game.Player.Character.Position) < 75)
            {
                if (Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, ped.Handle, Game.Player.Character.Handle, 17) && Vector3.Distance(ped.Position, Game.Player.Character.Position) < PullMeOverMain.range)
                {     
                    if (Game.Player.Character.IsInVehicle())
                    {
                        if (Game.Player.Character.CurrentVehicle.Equals(PullMeOverMain.fleevehicle))
                            Game.Player.WantedLevel = 1;
                        else if (Vector3.Distance(ped.Position, Game.Player.Character.Position) < 10)
                            Game.Player.WantedLevel = 1;
                    }
                    else
                        Game.Player.WantedLevel = 1;
                }     
            }

            if (Game.Player.WantedLevel == 0 && !end)
            {
                float dist = Vector3.Distance(ped.Position, nextpos);
                if(dist < 15 || ped.Velocity == Vector3.Zero)
                {
                    if (ped.IsInVehicle())
                        ped.Task.LeaveVehicle(ped.CurrentVehicle, false);
                    nextpos = World.GetSafeCoordForPed(new Vector3(PullMeOverMain.fleevehicle.Position.X + rand.Next(-65, 65), PullMeOverMain.fleevehicle.Position.Y + rand.Next(-65, 65), PullMeOverMain.fleevehicle.Position.Z), false);
                    ped.Task.RunTo(nextpos);
                } 
            }
            else
                ped.Task.ClearAll();
        }
    }

    public class BlipHandler
    {
        Blip cblip;
        Ped cped;
        DateTime ctimer;


        public BlipHandler(/*Blip blip, */Ped ped, DateTime time, bool iscop)
        {
            cped = ped;
            ctimer = time;

            if (!ped.CurrentBlip.Exists())
            {
                cped.AddBlip();
                cblip = ped.CurrentBlip;

                if (iscop)
                    cblip.Color = BlipColor.Blue;
                else
                    cblip.Color = BlipColor.Yellow;

                cblip.Scale = 0.5f;
            }
        }

        public bool IsVoided()
        {
            if (ctimer < DateTime.Now)
            {
                if (cped.Exists())
                    if (cped.CurrentBlip.Exists())
                        cped.CurrentBlip.Remove();
                return true;
            }
            return false;
        }

        public void Void()
        {
            if (cped.Exists())
                if (cped.CurrentBlip.Exists())
                    cped.CurrentBlip.Remove();
        }
    }
}

