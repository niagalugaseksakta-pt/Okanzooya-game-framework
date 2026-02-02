using Game.Model;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Agi", menuName = "RPG/Stat/AGI")]
public class CharacterAgiModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        EntityBase characterEntity = null;
        if (character != null)
        {
            characterEntity = character.gameObject.GetComponent<EntityBase>();
        }

        characterEntity.Stats.Agility += (int)Math.Round(val);

    }
}
