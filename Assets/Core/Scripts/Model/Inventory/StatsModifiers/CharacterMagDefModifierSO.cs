using Game.Model;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MagDef", menuName = "RPG/Stat/MAG_DEF")]
public class CharacterMagDefModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        EntityBase characterEntity = null;
        if (character != null)
        {
            characterEntity = character.gameObject.GetComponent<EntityBase>();
        }

        characterEntity.Stats.MagicDefense += (int)Math.Round(val);

    }
}
