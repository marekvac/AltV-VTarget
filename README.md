# VTarget

A 'third-eye' system inspired by FiveM resource qtarget (ox_target) for AltV.

This is abstracted version of my old roleplay framework, which I discontinued. And I think it still can be useful for someone, so I've decided to release it.

This resource is written in C# and unfortunately because of callback structure, cannot be used from js. Only way to use this resource is to add the dll file into your C# resource project and use API methods directly. The reason is mentioned above - this was intended to be part of bigger C# framework, so there was no need to make it work with javascript.

Resource is client-side only.

## How to run this resource

Add the resource to your C# project from NuGet.

Create instance of main VTarget `Target` class and add calls for OnStart, OnStop methods in your main class.

```C#
using MarcusCZ.AltV.VTarget.Target;

class YourResource
{
    private Target _vtarget;
    
    public void OnStart()
    {
        _vtarget = new Target();
        _vtarget.OnStart();
        // your code
    }
    
    public void OnStop()
    {
        // your code
        _vtarget.OnStop();
    }
}
```

After building your project, don't forget copy dll file of VTarget (`MarcusCZ.AltV.VTarget.Client.dll`) and downloaded frontend files into your resource. And include it both in resource.toml to client-files section.

## Concepts

- Option - is one option of displayed menu.
- Options can have children - that means, after clicking on one option, child menu can be displayed along with the main.
- Options can be `disabled` - disabled options have greyed background and player cannot click* on them.
- Options can have several conditions to display despite current circumstances.
  - `List<string> Bones` Player must aim at specific bone of entity.
  - `bool EnableInVehicle` If the option is enabled in vehicle. (ex. player must leave vehicle to interact with mailbox)
  - `float Distance` Defines maximum distance between targeted object and player. Default `7` max `100`
  - `bool callback CanShow` Custom callback, which is called before the option is displayed. Return value True|False defines if the option should be displayed in the menu or not.
  - `bool callback CanInterract` Custom callback, which is called after the option is displayed and decides whether the option is `disabled` or not.
- Option have several other attributes:
  - `string Icon` FontAwesome icon ID `fas fa-times`. Icon is displayed at the right side.
  - `string Label` Label of the option.
  - `Background? Background` Background color of the option. See `Background` enum in Data. Null = default (no) background.
  - `List<IVTargetOption>? Children` Children menu, displayed when clicked on the option.
  - ~~`string? Description`~~ NOT IMPLEMENTED
  - ~~`Position Position`~~ NOT IMPLEMENTED

*when option is disabled, callback OnClickDisabled is called instead of OnClick.
## Callbacks

### CanShow

Decides whether option can be displayed (sent to frontend) or not.

```c#
delegate bool VEntityOptionCheckCallback(uint entity, Vector3 pos, ISharedEntity? altEntity)
```

- `uint entity` - script entity ID, which player is aiming at.
- `Vector3 pos` - Hit position. Exact position where raycast from player camera hit the object, which player is aiming at.
- `ISharedEntity? altEntity` - If player is aiming at some entity (ped, other player, vehicle) created using AltV natives, their instance should be there. Can be null if player is aiming on some object for example.

<details>
  <summary>Example use</summary>

Display option only if the vehicle is unlocked.

  ```csharp
  // option will be displayed only when the vehicle is unlocked
  someDoorOption.CanShow = (entity, pos, altEntity) => {
    if (Alt.Natives.GetVehicleDoorLockStatus(entity) == 1) {
        return true;
    }
    return false;
  };    
  ```
</details>

### CanInteract
Decides whether the option is disabled or not. Similar like `CanShow`

````csharp
delegate bool VEntityOptionCheckCallback(uint entity, Vector3 pos, ISharedEntity? altEntity)
````

### OnClick
Called when player clicks on the option. Callback must return bool value - ``true|false``, which decides if the menu should be refreshed. That can be useful when we have options for opening vehicle doors, but we don't want to show them, when the vehicle is locked. We have another option for unlocking vehicle, so when the player unlocks the vehicle from this option, we want the other options to appear immediately, without closing and opening the menu again.

```csharp
delegate bool VEntityOptionCallback(uint entity, Vector3 pos, ISharedEntity? altEntity, Alert alert);
```
- `uint entity` - script entity ID, which player is aiming at.
- `Vector3 pos` - Hit position. Exact position where raycast from player camera hit the object, which player is aiming at.
- `ISharedEntity? altEntity` - If player is aiming at some entity (ped, other player, vehicle) created using AltV natives, their instance should be there. Can be null if player is aiming on some object for example.
- `Alert alert` - Function reference, which can render option alert in the VTarget menu.

<details>
  <summary>Example</summary>

If the vehicle is locked, unlock it, render alert and refresh the menu, so new options which were hidden until now, can show without reopening the menu.

  ```csharp
  // Option for unlocking vehicle
  option.OnClick = (entity, pos, altEntity, alert) => {
    if (Alt.Natives.GetVehicleDoorLockStatus(entity) != 1) { // If vehicle is locked
        Alt.Natives.SetVehicleDoorsLocked(entity, 1); // Unlock vehicle
        alert(Background.SUCCESS, "Vehicle was unlocked"); // Render alert with green background
        return true; // We want to refresh the menu
    }
    
    return false; // Vehicle is not locked, do nothing.
  };    
  ```
</details>

### OnDisabledClick
Called when option is ``disabled`` and player clicks on it. Same like `OnClick`

```csharp
delegate bool VEntityOptionCallback(uint entity, Vector3 pos, ISharedEntity? altEntity, Alert alert);
```

## Option registration for entities and objects

Using class `VEntityOption`

Available constructors:
```csharp
VEntityOption(string icon, string label); // ID will be generated random.
VEntityOption(string icon, string label, string id); // If we want specify ID.
```

Available methods for registration
```csharp
Target#RegisterGlobalVehicle(VEntityOption option); // Register option for all vehicles
Target#RegisterGlobalPlayer(VEntityOption option); // Register option for all players
Target#RegisterGlobalObject(uint model, VEntityOption option); // Register option for all objects with specified model.
// Target#Register(VEntityOption option); // Register for specific instance of AltV entity NOT IMPLEMENTED
```

### Example

Register option for opening vehicle doors.
```csharp
var option = new VEntityOption("fas fa-door", "Open doors");
option.Bones = new List<string> {"door_dside_f"}; // Option will be shown only if player aims at vehicle door.
option.CanInterract = (entity, pos, altEntity) => {
    if (Alt.Natives.GetVehicleDoorLockStatus(entity) == 1) {
        return true;
    }
    return false;
}; // option will be enabled only if the vehicle is unlocked
option.OnClick = (entity, pos, altEntity, alert) => {
    // open vehicle doors
    return false; // we dont need to refresh the menu
};
option.OnDisabledClick = (entity, pos, altEntity, alert) => {
    alert(Background.DANGER, "Vehicle is locked"); 
    return false;
};
_vtarget.RegisterGlobalVehicle(option); // Register this option for all vehicles
```

## Option registration for PolyZones

Please refer to my VZone resource.