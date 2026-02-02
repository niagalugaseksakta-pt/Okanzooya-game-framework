using Game.Model;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Def", menuName = "RPG/Stat/DEF")]
public class CharacterDefModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        EntityBase characterEntity = null;
        if (character != null)
        {
            characterEntity = character.gameObject.GetComponent<EntityBase>();
        }

        characterEntity.Stats.Defense += (int)Math.Round(val);

    }
}
