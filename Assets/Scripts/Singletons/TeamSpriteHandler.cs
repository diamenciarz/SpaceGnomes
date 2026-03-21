using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSpriteHandler : AbstractSingleton<TeamSpriteHandler>
{
    public enum Color
    {
        Red,
        Blue,
        Green
    }

    [Serializable]
    public class SpriteTriple
    {
        public Sprite redSprite;
        public Sprite blueSprite;
        public Sprite greenSprite;
    }

    [Tooltip("Set of sprites with 3 colors (red, blue, green) for each team.")]
    public List<SpriteTriple> spriteTriples = new List<SpriteTriple>();
    public void UpdateSprite(SpriteRenderer renderer, EntityTeam.Team team)
    {
        Color color = GetTeamColor(team);
        Sprite coloredSprite = GetColoredSprite(renderer.sprite, color);
        renderer.sprite = coloredSprite;
    }
    private Color GetTeamColor(EntityTeam.Team team)
    {
        switch (team)
        {
            case EntityTeam.Team.Team1:
                return Color.Blue;
            case EntityTeam.Team.Neutral:
                return Color.Green;
            default:
                return Color.Red; // Default to red for Team2, Team3, Team4, Team5, Team6, Team7, Team8, and EnemyToAll
        }
    }
    private Sprite GetColoredSprite(Sprite baseSprite, Color color)
    {
        if(GetSpriteTriple(baseSprite, out SpriteTriple triple))
        {
            switch (color)
            {
                case Color.Red:
                    return triple.redSprite;
                case Color.Blue:
                    return triple.blueSprite;
                case Color.Green:
                    return triple.greenSprite;
                default:
                    return triple.redSprite;
            }
        }
        else
        {
            return baseSprite; // If not found, return the original sprite
        }
    }
    private bool GetSpriteTriple(Sprite baseSprite, out SpriteTriple outputTriple)
    {
        foreach (var triple in spriteTriples)
        {
            if (triple.redSprite == baseSprite || triple.blueSprite == baseSprite || triple.greenSprite == baseSprite)
            {
                outputTriple = triple;
                return true;
            }
        }
        outputTriple = null;
        return false;
    }
}
