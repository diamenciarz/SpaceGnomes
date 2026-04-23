using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityTeam;
public class TeamSwitcherTest : AbstractActionController
{
    public override void Detach() { }
    public override void Deactivate() { }
    public override void Activate()
    {
        EntityTeam entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
        TeamManager.Instance.SetEntityTeam(gameObject, entityTeam.team == Team.Team1 ? Team.Team2 : Team.Team1);
    }
}
