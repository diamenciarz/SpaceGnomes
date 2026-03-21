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

    public override void SetAction(bool isOn)
    {
        if (isOn)
        {
            EntityTeam entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
            TeamManager.Instance.ChangeEntityTeam(gameObject, entityTeam.team == Team.Team1 ? Team.Team2 : Team.Team1);
        }
    }
}
