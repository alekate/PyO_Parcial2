using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public string TileName;
    [SerializeField] protected SpriteRenderer _renderer;

    [Header("Highlight Objects")]
    [SerializeField] private GameObject _hoverHighlight;
    [SerializeField] private GameObject _moveHighlight;
    [SerializeField] private GameObject _rangeAttackHighlight;
    [SerializeField] private GameObject _meleeAttackHighlight;

    [Header("Tile Properties")]
    [SerializeField] private bool _isWalkable;

    public BaseUnit OccupiedUnit;
    public bool Walkable => _isWalkable && OccupiedUnit == null;

    public int X { get; private set; }
    public int Y { get; private set; }

    // Para detectar doble click
    private float _lastClickTime;
    private const float _doubleClickThreshhold = 0.3f;

    public enum TileHighlightType
    {
        None,
        Hover,
        Move,
        RangeAttack,
        MeleeAttack
    }

    public virtual void Init(int x, int y)
    {
        X = x;
        Y = y;
    }

    void OnMouseEnter()
    {
        SetHighlight(TileHighlightType.Hover, true);
        MenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit()
    {
        SetHighlight(TileHighlightType.Hover, false);
        MenuManager.Instance.ShowTileInfo(null);
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.HeroesTurn)
            return;

        var selectedHero = UnitManager.Instance.SelectedHero;

        // 1) Doble click en el mismo héroe → curar
        if (OccupiedUnit == selectedHero && !selectedHero.HasActed)
        {
            if (Time.time - _lastClickTime < _doubleClickThreshhold)
            {
                Debug.Log("Curar self");
                selectedHero.Heal(selectedHero.healAmount);
                selectedHero.SetHasActed(true);
                EndHeroAction();
                return;
            }
            _lastClickTime = Time.time;
        }

        // 2) Si el tile tiene una unidad
        if (OccupiedUnit != null)
        {
            // 2a) Seleccionar héroe
            if (OccupiedUnit.Faction == Faction.Hero)
            {
                GridManager.Instance.ClearAllHighlights();
                UnitManager.Instance.SetSelectedHero((BaseHero)OccupiedUnit);
                return;
            }

            // 2b) Atacar enemigo
            if (selectedHero != null && !selectedHero.HasActed)
            {
                var meleeTiles = GridManager.Instance.GetTilesInRange(
                    selectedHero.OccupiedTile,
                    selectedHero.meleeAttackDistance,
                    false);

                var rangeTiles = GridManager.Instance.GetTilesInRange(
                    selectedHero.OccupiedTile,
                    selectedHero.rangeAttackDistance,
                    false);

                if (meleeTiles.Contains(this))
                {
                    Debug.Log("Melee attack!");
                    OccupiedUnit.TakeDamage(selectedHero.meleeAttackDamage);
                    selectedHero.SetHasActed(true);
                }
                else if (rangeTiles.Contains(this))
                {
                    Debug.Log("Ranged attack!");
                    OccupiedUnit.TakeDamage(selectedHero.rangeAttackDamage);
                    selectedHero.SetHasActed(true);
                }

                // Revisar victoria/derrota tras el ataque
                UnitManager.Instance.CheckGameOver();

                EndHeroAction();
                return;
            }
        }
        else
        {
            // 3) Mover héroe
            if (selectedHero != null && Walkable && !selectedHero.HasActed)
            {
                var moveTiles = GridManager.Instance.GetTilesInRange(
                    selectedHero.OccupiedTile,
                    selectedHero.velocity,
                    true);

                if (moveTiles.Contains(this))
                {
                    SetUnit(selectedHero);
                    selectedHero.SetHasActed(true);
                    EndHeroAction();
                }
            }
        }
    }

    public void SetHighlight(TileHighlightType type, bool state)
    {
        switch (type)
        {
            case TileHighlightType.Hover:
                _hoverHighlight?.SetActive(state);
                break;
            case TileHighlightType.Move:
                _moveHighlight?.SetActive(state);
                break;
            case TileHighlightType.RangeAttack:
                _rangeAttackHighlight?.SetActive(state);
                break;
            case TileHighlightType.MeleeAttack:
                _meleeAttackHighlight?.SetActive(state);
                break;
        }
    }

    public void SetUnit(BaseUnit unit)
    {
        if (unit == null)
        {
            OccupiedUnit = null;
            return;
        }

        if (unit.OccupiedTile != null)
            unit.OccupiedTile.OccupiedUnit = null;

        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;
    }

    // Centraliza limpieza, cambio de selección, chequeo de fin de juego y cambio de turno
    private void EndHeroAction()
    {
        GridManager.Instance.ClearAllHighlights();
        UnitManager.Instance.SetSelectedHero(null);

        UnitManager.Instance.CheckGameOver();

        if (UnitManager.Instance.AllHeroesHaveActed())
        {
            Debug.Log("Todos los héroes actuaron");
            GameManager.Instance.ChangeState(GameState.EnemiesTurn);
        }
    }
}
