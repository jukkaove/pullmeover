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
        List<Ped> potentialcops = new List<Ped>();
        internal static void FindCops()
        {
            for (int i = 0; i < coplist.Count; i++)
            {
                if (!coplist[i].Exists())
                    coplist.Remove(coplist[i]);
            }
        
            copsi = World.GetNearbyPeds(Game.Player.Character.Position, 100);

            for (int i = 0; i < copsi.Length; i++)
            {
                if (coplist.Contains(copsi[i]))
                    continue;
                if ((Function.Call<int>(Hash.GET_PED_TYPE, copsi[i]) == 6 || addonpeds.Contains(copsi[i].Model.Hash)) && !copsi[i].Equals(Game.Player.Character))
                    coplist.Add(copsi[i]); 
            }  
        }

        Ped GetClosest(bool setchase, bool changechaser)
        {
            float closest = 500;
            float distance = 0;
            Ped cop = null;
            for (int i = 0; i < potentialcops.Count; i++)
            {
                distance = Vector3.Distance(potentialcops[i].Position, Game.Player.Character.Position);
                
                if (distance < closest)
                {
                    if (setchase || changechaser)
                    {
                        if (!potentialcops[i].IsInVehicle())
                            continue;
                        if (!potentialcops[i].CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver).Equals(potentialcops[i]))
                            continue;
                        if (setchase && !changechaser)
                            copped = potentialcops[i];
                        if (changechaser)
                            newcop = potentialcops[i];
                    }
                    closest = distance;
                    cop = potentialcops[i];
                }
            }
            return cop;
        }
        
        Ped GetCop(int rangex, bool cansee, bool driver, bool pedcop, bool setchase, bool changechaser)
        {
            bool firstloop = false;
            potentialcops.Clear();
            for (int i = 0; i < coplist.Count; i++)
            {
                firstloop = true;
                float dist = Vector3.Distance(coplist[i].Position, Game.Player.Character.Position);

                if (driver)
                {
                    if (!coplist[i].IsInVehicle())
                        continue;
                    if (!coplist[i].CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver).Equals(coplist[i]))
                        continue;
                    if (coplist[i].CurrentVehicle.SirenActive)
                        continue;
                }
                if (cansee)
                {
                    if (!Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, coplist[i].Handle, Game.Player.Character.Handle, 17) || dist > rangex)
                        continue;
                }

                potentialcops.Add(coplist[i]);
            }

            if (firstloop && potentialcops.Count != 0)
                return GetClosest(setchase, changechaser);
            
            if (pedcop && settingcanbereported)
            {
                Ped witness = GetCop(40, true, false, false, false, false);
                if (witness != null)
                {
                    if (witness.IsOnFoot)
                        witness.Task.TurnTo(Game.Player.Character, 6000);

                    Function.Call(Hash._PLAY_AMBIENT_SPEECH2, witness, "SUSPECT_SPOTTED", "SPEECH_PARAMS_STANDARD");
                    bliplist.Add(new BlipHandler(witness, DateTime.Now.AddSeconds(6), true));
                    return GetCop(400, false, true, false, true, false);
                }
            }
            return null;
        }
    }
}
