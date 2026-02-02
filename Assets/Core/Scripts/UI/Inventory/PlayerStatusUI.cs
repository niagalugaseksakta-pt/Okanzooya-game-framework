using Game.Model.Player;
using TMPro;
using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    public TextMeshProUGUI atkText, defText, magDefText, magText, agiText, manaText, expText, coinText, diamondText, levelText;
    public UnityEngine.UI.Image equippedIcon; // Current set equipped icon
    public UnityEngine.UI.Image settedEquippedIcon; // Latest equipped icon to set

    private int atk, def, magDef, mag, agi, mana, exp, coin, diamond, level;
    public void Refresh(PlayerEntity playerEntity, Sprite equipImage)
    {
        settedEquippedIcon.sprite = equipImage;
        equippedIcon.sprite = settedEquippedIcon.sprite;
        atkText.text = $"ATK : {playerEntity.Stats.AttackPower.ToString()}";
        defText.text = $"DEF : {playerEntity.Stats.Defense.ToString()}";
        magDefText.text = $"MAG DEF : {playerEntity.Stats.MagicDefense.ToString()}";
        magText.text = $"MAG : {playerEntity.Stats.MagicPower.ToString()}";
        agiText.text = $"AGI : {playerEntity.Stats.Agility.ToString()}";
        manaText.text = $"MANA : {playerEntity.Stats.Mana.ToString()}";
        expText.text = $"EXP : {playerEntity.Stats.Experience.ToString()}";

        //levelText.text = playerEntity.Stats.Level.ToString();
        //coinText.text = playerEntity.Stats.Coin.ToString();
        //diamondText.text = playerEntity.Stats.Diamond.ToString();
    }
}
