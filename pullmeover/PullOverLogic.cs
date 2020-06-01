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
        int SafeDistance()
        {
            float distance = 0;
            float traileri = 0;
            float auto = 0;
            if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, Game.Player.Character.CurrentVehicle))
            {
                OutputArgument trailer = new OutputArgument();
                Function.Call<Entity>(Hash.GET_VEHICLE_TRAILER_VEHICLE, Game.Player.Character.CurrentVehicle, trailer);
                Entity temp = trailer.GetResult<Entity>();

                if (temp != null)
                {
                    traileri = temp.Model.GetDimensions().Y;
                    distance += temp.Model.GetDimensions().Y;
                }
            }

            auto = playerVehicle.Model.GetDimensions().Y;
            distance += playerVehicle.Model.GetDimensions().Y;

            return (int)Math.Ceiling(distance);
        }
        
        void Action()
        {
            while (!IsInterrupted())
            {
                if (!Function.Call<bool>(Hash.IS_PED_FACING_PED, Game.Player.Character, copped, 100f))
                    break;
                else if (Vector3.Distance(copped.Position, Game.Player.Character.Position) > (range + 50) && playerVehicle.Speed < copveh.Speed)
                    break;
                else if (waschanged)
                    break;

                Wait(0);
            }

            int stopdistance = SafeDistance();
            
            Function.Call(Hash.TASK_VEHICLE_CHASE, copped, Game.Player.Character.Handle);
            Function.Call(Hash.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE, copped, 25f);
            Function.Call(Hash.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG, copped, 32, true);
            Function.Call(Hash.SET_DRIVER_ABILITY, copped, 0.9f);

            copped.Weapons.RemoveAll();

            if (!settingapproachwithlights || waschanged)
            {
                Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, copveh, true);
                copveh.SirenActive = true;
            }

            bool ticket = false;
            bool lights = false;

            float dist = Vector3.Distance(Game.Player.Character.Position, copped.Position);
            ticks = 0;
            int visualticks = 0;
            bool changecop = false;

            //Here is the pursuit logic. The pursuit behaviour changes based on distance. It takes into account player's vehicle size
            while (!IsInterrupted())
            {

                dist = Vector3.Distance(Game.Player.Character.Position, copped.Position);
                ExcessSpeeding(range, 0);

                if (!Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, copped.Handle, Game.Player.Character.Handle, 17))
                {
                    visualticks++;
                    if (visualticks > 140 && dist > 100)
                        break;
                }
                else
                    visualticks = 0;

                if (settingcopcarchange && ((dist > 100 && copveh.Speed < 10) || (dist > 150) && ticks % 5 == 0))
                {
                    if (GetCop(30, true, true, false, true, true) != null)
                    {
                        waschanged = true;
                        changecop = true;
                        break;
                    }
                }

                //Managing pursuit when the distance between cop and player is under 100
                if (dist < 100)
                {
                    ticks++;
                    if (playerVehicle.Speed == 0)
                        ticks--;

                    if (dist < stopdistance+20 && !lights && Function.Call<bool>(Hash.IS_PED_FACING_PED, copped, Game.Player.Character, 90f) && !waschanged && ChaserCanSee())
                    {
                        if (settingapproachwithlights)
                        {
                            Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, copveh, true);
                            copveh.SirenActive = true;
                        }

                        Function.Call(Hash._PLAY_AMBIENT_SPEECH2, Game.Player.Character, "GENERIC_CURSE_MED", "SPEECH_PARAMS_FORCE");
                        Function.Call(Hash.BLIP_SIREN, copveh);
                        wasblipped = true;
                        lights = true;
                    }
                    if (dist >= stopdistance + 18 && state != CopStates.Chase)
                    {
                        state = CopStates.Chase;
                        Function.Call(Hash.TASK_VEHICLE_CHASE, copped, Game.Player.Character.Handle);
                        Function.Call(Hash.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE, copped, 25f);
                        Function.Call(Hash.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG, copped, 32, true);

                        copped.Weapons.RemoveAll();
                    }
                    else if (dist < stopdistance + 18 && state != CopStates.Follow)
                    {
                        state = CopStates.Follow;
                        Function.Call(Hash._TASK_VEHICLE_FOLLOW, copped, copveh, playerVehicle.Handle, 100, 0, 5+ stopdistance);
                    }
                    else if (dist < 8 + stopdistance && playerVehicle.Speed < 1 && copveh.Speed < 2)
                    {
                        int tticks = 0; ;
                        while (playerVehicle.Speed < 1 && !IsInterrupted() && (dist < 12 + stopdistance) && tticks < 10)
                        {
                            Wait(500);
                            tticks++;
                        }
                        if (tticks >= 10)
                        {
                            ticket = true;
                            break;
                        }
                    }

                    //Prevents cop from slamming into player
                    if (dist < stopdistance + 3 && copveh.Speed > playerVehicle.Speed && playerVehicle.Speed < 3)
                        copveh.Speed = 0f;

                    //Pursuit has taken long enough
                    if (ticks >= 200)
                    {
                        Game.Player.WantedLevel = 1;
                        break;
                    }

                    //Plays megaphone sounds on interval
                    if (dist < 30 && ticks % 40 == 0)
                    {
                        if (ticks >= 160)
                        {
                            Function.Call(Hash.BLIP_SIREN, copveh);
                            Wait(500);
                            Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "STOP_VEHICLE_CAR_WARNING_MEGAPHONE", "SPEECH_PARAMS_FORCE");
                            ticks++;
                        }
                        else
                        {
                            Function.Call(Hash.BLIP_SIREN, copveh);
                            wasblipped = true;
                            Wait(500);
                            Function.Call(Hash._PLAY_AMBIENT_SPEECH2, copped, "STOP_VEHICLE_GENERIC_MEGAPHONE", "SPEECH_PARAMS_FORCE");
                            ticks++;
                        }
                    }
                }

                else if (dist > 350)
                    break;

                Wait(0);
            }


            if (ticket)
                TicketTime(copveh, copped);
            else if (eventstate == EventState.Locate)
                SendCops();
            else if (changecop)
            {
                EndEvent(true);
                SetChase();
            }

            else
            {
                if (wasblipped)
                    evasionlist.Add(playerVehicle);
                copped.Task.ClearAll();
                EndEvent(false);
            }
        }
    }
}
