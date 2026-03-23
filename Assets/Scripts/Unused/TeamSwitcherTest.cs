using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityTeam;
public class TeamSwitcherTest : AbstractActionController
{
    public override void Detach()
    {
        
    }

    public override ShipAction GetActionType()
    {
        return ShipAction.ShootUltimate;
    }

    public override void SetAction(bool isOn, GameObject optionalTarget)
    {
        if (isOn)
        {
            EntityTeam entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
            TeamManager.Instance.SetEntityTeam(gameObject, entityTeam.team == Team.Team1 ? Team.Team2 : Team.Team1);
        }
    }
}
