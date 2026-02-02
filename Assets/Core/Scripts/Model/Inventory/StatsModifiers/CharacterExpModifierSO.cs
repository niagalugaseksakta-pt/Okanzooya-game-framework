using Game.Model;
using Game.Model.Player;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Exp", menuName = "RPG/Stat/EXP")]
public class CharacterExpModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        EntityBase characterEntity = null;
        if (character != null)
        {
            characterEntity = character.gameObject.GetComponent<PlayerEntity>();
        }

        characterEntity.Stats.Experience += (int)Math.Round(val);

    }
}
