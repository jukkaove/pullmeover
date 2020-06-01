using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;

namespace PullMeOver
{
    public partial class PullMeOverMain : Script
    {
        internal bool RunningRedLight()
        {
            if (!redlight)
                return false;

            vehs = World.GetNearbyVehicles(Game.Player.Character.Position, 9);

            if (vehs.Length == 0)
                return false;

            for (int i = 0; i < vehs.Length; i++)
            {
                if (autolist.Count == 0)
                {
                    autolist.Add(vehs[i]);
                    continue;
                }
                if (!autolist.Contains(vehs[i]))
                    autolist.Add(vehs[i]);
            }

            if (autolist.Count == 0)
                return false;

            for (int i = 0; i < autolist.Count; i++)
            {
                if (Vector3.Distance(autolist[i].Position, Game.Player.Character.Position) > 35)
                    autolist.Remove(autolist[i]);
            }

            for (int i = 0; i < autolist.Count; i++)
            {
                if (!Function.Call<bool>(Hash.IS_VEHICLE_STOPPED_AT_TRAFFIC_LIGHTS, autolist[i]))
                    continue;

                float angle = Vector3.Angle(autolist[i].ForwardVector, Game.Player.Character.ForwardVector);

                if (angle > 35)
                    continue;
                else if (angle <= 35 && Vector3.Distance(autolist[i].Position, Game.Player.Character.Position) > 30)
                {
                    Vector3 cardir = autolist[i].Position - Game.Player.Character.Position;
                    // UI.Notify(Vector3.Angle(cardir, Game.Player.Character.ForwardVector) + "");

                    if (Vector3.Angle(cardir, Game.Player.Character.ForwardVector) > 150)
                    {
                        //   UI.ShowSubtitle("RUNNING A RED LIGHT!!", 2000);
                        return true;
                    }

                }

            }
            return false;

        }

        internal bool DrivingAgainstTraffic(int dat)
        {
            if (!settingagainsttraffic)
                return false;
            if (playerVehicle.Speed < 8)
            {
                againsttrafficticks = 0;
                return false;
            }

            if (dat == 0)
                againsttrafficticks++;

            else
                againsttrafficticks = 0;

            if (againsttrafficticks > 8)
            {
               // UI.ShowSubtitle("at " + againsttrafficticks);
                infractionlocation = World.GetStreetName(Game.Player.Character.Position);
                return true;
            }
            
            return false;

        }

        internal bool WithoutHelmet()
        {
            if (!settingwithouthelmet)
                return false;
            if (stoppedforhelmet)
                return false;

            if (playerVehicle.Model.IsBike && playerVehicle.Speed > 3 && !playerVehicle.Model.IsBicycle)
                if (!Game.Player.Character.IsWearingHelmet)
                    return true;
            return false;
        }

        internal bool UsingMobilePhone()
        {
            if (!settingmobilephone)
                return false;
            if (!Function.Call<bool>(Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, Game.Player.Character))
                return false;
            if (!playerVehicle.GetPedOnSeat(VehicleSeat.Driver).Equals(Game.Player.Character))
                return false;
            return true;
        }

        internal bool Wheelie()
        {
            if (!settingwheelie)
                return false;
            if (playerVehicle.Model.IsBike)
            {
                if (!playerVehicle.IsInAir && !playerVehicle.IsOnAllWheels)
                {
                    wheelieticks++;
                    //UI.ShowSubtitle("wheelie " + wheelieticks);
                    if (wheelieticks > 10)  
                        return true;
                    return false;
                }
                wheelieticks = 0;
                return false;
            }
            wheelieticks = 0;
            return false;
        }

        internal bool NotRoadworthy()
        {
            if (!settingnotrw)
                return false;
            if (stoppedforcarfix)
                return false;

            if (playerVehicle.BodyHealth < 700)
                return true;

            return false;
        }

        internal bool Tailgating()
        {
            if (!settingtailgating)
                return false;
            if (playerVehicle.Speed < 10)
                return false;

            RaycastResult k = World.Raycast(Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 5, IntersectOptions.Everything);

            if (k.DitHitEntity)
            {
                Entity tmp = k.HitEntity;
                String tm = tmp.GetType().ToString();

                if (tm.Contains("Vehicle"))
                {
                    tailgatingticks++;
                 //   UI.ShowSubtitle("tg " + tailgatingticks);
                    Vehicle vehi = (Vehicle)tmp;
                    if (vehi.Equals(playerVehicle))
                        return false;
                    if (vehi.Speed < 10)
                        return false;
                    if (tailgatingticks > 15)
                        return true;


                }
                else
                    tailgatingticks = 0;

            }
            else
                tailgatingticks = 0;

            return false;
        }

        internal bool Sliding()
        {
            if (!settingdrifting)
                return false;
            if (!playerVehicle.Model.IsCar && !playerVehicle.Model.IsQuadbike)
                return false;
            if (prevpos == null || playerVehicle.Speed < 4)
            {
                prevpos = Game.Player.Character.Position;
                return false;
            }

            Vector3 dir = (Game.Player.Character.Position - prevpos).Normalized;
            float angle = Vector3.Angle(dir, Game.Player.Character.ForwardVector);
            
            if (angle > 20 && angle < 120)
            {
                slidingticks++;
                //UI.ShowSubtitle(slidingticks + " " + angle);
                prevpos = Game.Player.Character.Position;
                if (slidingticks > 4)
                {
                      
                    return true;
                }
                else
                    return false;
            }
            slidingticks = 0;
            return false;
        }

        internal bool DrivingOnPavement()
        {
            if (!settingonsidewalk)
                return false;
            if (playerVehicle.Speed < 10)
                return false;
            if (Vector3.Distance(World.GetSafeCoordForPed(Game.Player.Character.Position, true), Game.Player.Character.Position) < 0.7f)
            {
                return true;
            }

            return false;
        }

        internal bool StopSignViolation()
        {
            if (!settingstopsign)
                return false;
            Prop[] props = World.GetNearbyProps(Game.Player.Character.Position, 10);

            if (stopsign == null)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    if (props[i].Model.Hash == -949234773)
                        if (Vector3.Angle(Game.Player.Character.ForwardVector, props[i].ForwardVector) < 15)
                        {
                            if (debugss)
                                UI.Notify("angle = " + Vector3.Angle(Game.Player.Character.ForwardVector, props[i].ForwardVector) + "");
                            stopsign = props[i];
                        }
                }
                return false;
            }
            else
            {
                if (playerVehicle.Speed < stopsignspeed)
                    stopsignspeed = playerVehicle.Speed;
                //    UI.ShowSubtitle(Vector3.Distance(Game.Player.Character.Position, stopsign.Position) + " " +stopsignspeed);
                if (Vector3.Distance(Game.Player.Character.Position, stopsign.Position) > 10)
                {
                    Prop[] props2 = World.GetNearbyProps(Game.Player.Character.Position, 20);
                    for (int i = 0; i < props2.Length; i++)
                    {
                        if (trafficlights.Contains(props2[i].Model.Hash))
                        {
                            if (debugss)
                                UI.ShowSubtitle("stop sign ignored: traffic lights");
                            stopsign = null;
                            stopsignspeed = 100;
                            return false;
                        }
                    }
                    if (Vector3.Angle(Game.Player.Character.ForwardVector, stopsign.ForwardVector) < 110 && stopsignspeed >= 1.5f)
                    {
                        if (debugss)
                            UI.ShowSubtitle("Ran a stop sign");
                        stopsign = null;
                        stopsignspeed = 100;
                        return true;
                    }
                    else
                    {
                        if (debugss)
                            UI.ShowSubtitle("no violation");
                        stopsign = null;
                        stopsignspeed = 100;
                        return false;
                    }

                }
            }
            return false;
        }

        internal bool Burnout()
        {
            if (Function.Call<bool>(Hash.IS_VEHICLE_IN_BURNOUT, playerVehicle))
                burnoutticks++;
            else
                burnoutticks = 0;
          //  UI.ShowSubtitle(burnoutticks + " " + playerVehicle.Speed);
            if (burnoutticks > 500)
                return true;
            return false;
        }
    }
}
