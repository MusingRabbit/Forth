using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IPlayerController
{
    Vector2 LookAxis { get; set; }
    Vector2 MoveAxis { get; set; }
    int Drop { get; set; }
    int EquipMain { get; set; }
    int EquipPack { get; set; }
    int EquipSide { get; set; }
    int GravBoots { get; set; }
    int Jump { get; set; }
    int Melee { get; set; }
    int Throw { get; set; }
    int Trigger { get; set; }
    int Use { get; set; }

    ActionState GetActionState(ControllerActions action);
    void Reset();
    void SetActionState(ControllerActions action, ActionState state = ActionState.Active);
    void Start();
    void SetStateFromNetPlayerController(NetPlayerController netController);
    NetPlayerController GetNetPlayerController();
    List<ControllerActions> GetActiveControllerActions();
}