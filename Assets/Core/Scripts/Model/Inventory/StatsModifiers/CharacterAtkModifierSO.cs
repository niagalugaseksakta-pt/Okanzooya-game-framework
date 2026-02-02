using Game.Model;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Atk", menuName = "RPG/Stat/ATK")]
public class CharacterAtkModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        EntityBase characterEntity = null;
        if (character != null)
        {
            characterEntity = character.gameObject.GetComponent<EntityBase>();
        }

        characterEntity.Stats.AttackPower += (int)Math.Round(val);

    }
}
