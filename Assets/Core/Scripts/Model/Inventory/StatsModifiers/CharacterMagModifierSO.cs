using Game.Model;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Mag", menuName = "RPG/Stat/MAG")]
public class CharacterMagModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        EntityBase characterEntity = null;
        if (character != null)
        {
            characterEntity = character.gameObject.GetComponent<EntityBase>();
        }

        characterEntity.Stats.MagicPower += (int)Math.Round(val);

    }
}
