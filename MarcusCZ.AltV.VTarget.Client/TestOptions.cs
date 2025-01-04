using AltV.Net.Client;
using AltV.Net.Enums;
using MarcusCZ.AltV.VTarget.Client.Data;
using MarcusCZ.AltV.VTarget.Client.Options;

namespace MarcusCZ.AltV.VTarget.Client;

public class TestOptions
{
    public static void RegisterTestOptions()
    {
        VEntityOption unlock = new VEntityOption("fas fa-lock", "Unlock Vehicle")
        {
            CanShow = (entity, _, _) => Alt.Natives.GetVehicleDoorLockStatus(entity) != 1,
            OnClick = (entity, _, _, alert) =>
            {
                Alt.Natives.SetVehicleDoorsLocked(entity, 1);
                alert(Background.SUCCESS, "Vehicle unlocked");
                return true;
            }
        };
        VEntityOption lockOpt = new VEntityOption("fas fa-lock", "Lock Vehicle")
        {
            CanShow = (entity, _, _) => Alt.Natives.GetVehicleDoorLockStatus(entity) == 1,
            OnClick = (entity, _, _, alert) =>
            {
                Alt.Natives.SetVehicleDoorsLocked(entity, 2);
                alert(Background.DANGER, "Vehicle locked");
                return true;
            }
        };
        VEntityOption openDoor = new VEntityOption("fas fa-check", "Open door")
        {
            Bones = new List<string> {"door_dside_f"},
            CanShow = (entity, _, _) => Alt.Natives.GetVehicleDoorLockStatus(entity) == 1,
            OnClick = (entity, _, _, _) =>
            {
                Alt.Natives.SetVehicleDoorOpen(entity, (int) VehicleDoor.DriverFront, false, true);
                return false;
            },
            OnDisabledClick = (_, _, _, alert) =>
            {
                alert(Background.DANGER, "Vehicle is locked!");
                return false;
            }
        };
        VEntityOption exampleChildren = new VEntityOption("fas fa-info", "Children menu")
        {
            Children = new List<IVTargetOption>
            {
                new VEntityOption("fas fa-pencil", "Some opt"),
                new VEntityOption("fas fa-check", "Another menu..")
                {
                    Children = new List<IVTargetOption>
                    {
                        new VEntityOption("fas fa-clock", "Success bg")
                        {
                            Background = Background.SUCCESS
                        },
                        new VEntityOption("fas fa-clock", "Danger bg")
                        {
                            Background = Background.DANGER
                        },
                        new VEntityOption("fas fa-clock", "Info bg")
                        {
                            Background = Background.INFO
                        },
                        new VEntityOption("fas fa-clock", "Warning bg")
                        {
                            Background = Background.WARNING
                        },
                        new VEntityOption("fas fa-clock", "Primary bg")
                        {
                            Background = Background.PRIMARY
                        },
                        new VEntityOption("fas fa-clock", "Purple bg")
                        {
                            Background = Background.PURPLE
                        }
                    }
                },
                new VEntityOption("fas fa-times", "Disabled option")
                {
                    CanInteract = (_, _, _) => false,
                    OnDisabledClick = (_, _, _, alert) =>
                    {
                        alert(Background.WARNING, "This option is disabled :(");
                        return false;
                    }
                }
            }
        };
        
        Target.RegisterGlobalVehicle(lockOpt);
        Target.RegisterGlobalVehicle(unlock);
        Target.RegisterGlobalVehicle(openDoor);
        Target.RegisterGlobalObject(1363150739, exampleChildren);
    }
}