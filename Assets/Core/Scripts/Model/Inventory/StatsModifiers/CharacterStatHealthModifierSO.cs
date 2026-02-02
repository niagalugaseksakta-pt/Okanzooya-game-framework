using Game.Model.Player;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Health", menuName = "RPG/Stat/HEALTH")]
public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        character.GetComponent<PlayerEntity>().Stats.CurrentHealth += Convert.ToInt32(Math.Abs(val));
    }
}
