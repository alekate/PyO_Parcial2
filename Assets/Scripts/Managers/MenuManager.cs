using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private GameObject _selectedHeroObject, _tileObject, _tileUnitObject, _endgame;

    void Awake()
    {
        Instance = this;
    }

    public void ShowTileInfo(Tile tile)
    {
        if (tile == null)
        {
            _tileObject.SetActive(false);
            _tileUnitObject.SetActive(false);
            return;
        }

        TextMeshProUGUI tileText = _tileObject.GetComponentInChildren<TextMeshProUGUI>();
        if (tileText != null)
        {
            tileText.text = tile.TileName;
        }
        _tileObject.SetActive(true);

        if (tile.OccupiedUnit)
        {
            TextMeshProUGUI unitText = _tileUnitObject.GetComponentInChildren<TextMeshProUGUI>();
            if (unitText != null)
            {
                unitText.text = tile.OccupiedUnit.UnitName;
            }
            _tileUnitObject.SetActive(true);
        }
        else
        {
            _tileUnitObject.SetActive(false);
        }
    }

    public void ShowSelectedHero(BaseHero hero)
    {
        if (hero == null)
        {
            _selectedHeroObject.SetActive(false);
            return;
        }

        TextMeshProUGUI heroText = _selectedHeroObject.GetComponentInChildren<TextMeshProUGUI>();
        if (heroText != null)
        {
            heroText.text = $"Player: {hero.UnitName}\n" +
                $"HP: {hero.health}\n" +
                $"Velocity: {hero.velocity}\n" +
                $"Melee: {hero.meleeAttackDamage} dmg ({hero.meleeAttackDistance} range)\n" +
                $"Ranged: {hero.rangeAttackDamage} dmg ({hero.rangeAttackDistance} range)\n" +
                $"Heal: {hero.healAmount} ({hero.healDistance} range)";

        }
        _selectedHeroObject.SetActive(true);
    }

    public void ShowVictory()
    {
        TextMeshProUGUI resultText = _endgame.GetComponentInChildren<TextMeshProUGUI>();

        resultText.text = "YOU WIN";

        _endgame.SetActive(true);
    }

    public void ShowDefeat()
    {
        TextMeshProUGUI resultText = _endgame.GetComponentInChildren<TextMeshProUGUI>();

        resultText.text = "YOU LOSE";

        _endgame.SetActive(true);
    }

}
