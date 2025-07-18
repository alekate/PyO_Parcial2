using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Tile;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    private List<BaseHero> _heroes = new List<BaseHero>();
    private List<BaseEnemy> _enemies = new List<BaseEnemy>();

    [SerializeField] private BaseHero[] heroPrefabs;
    [SerializeField] private BaseEnemy[] enemyPrefabs;
    [SerializeField] private Transform heroesParent;
    [SerializeField] private Transform enemiesParent;

    private BaseHero _selectedHero;
    public BaseHero SelectedHero => _selectedHero;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnHeroes()
    {
        _heroes.Clear();
        foreach (var prefab in heroPrefabs)
        {
            var hero = Instantiate(prefab, heroesParent);
            var tile = GridManager.Instance.GetHeroSpawnTile();
            tile.SetUnit(hero);
            hero.SetTile(tile);
            hero.Init();
            _heroes.Add(hero);
        }

        GameManager.Instance.ChangeState(GameState.SpawnEnemies);
    }

    public void SpawnEnemies()
    {
        _enemies.Clear();
        foreach (var prefab in enemyPrefabs)
        {
            var enemy = Instantiate(prefab, enemiesParent);
            var tile = GridManager.Instance.GetEnemySpawnTile();
            tile.SetUnit(enemy);
            enemy.SetTile(tile);
            enemy.Init();
            _enemies.Add(enemy);
        }

        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }

    public void SetSelectedHero(BaseHero hero)
    {
        _selectedHero = hero;
        GridManager.Instance.ClearAllHighlights();

        if (hero == null)
        {
            MenuManager.Instance.ShowSelectedHero(hero);
            return;
        }

        if (GameManager.Instance.GameState != GameState.HeroesTurn)
            return;

        MenuManager.Instance.ShowSelectedHero(hero);

        // Destacar movimiento
        var moveTiles = GridManager.Instance.GetTilesInRange(
            hero.OccupiedTile, hero.velocity, true);
        foreach (var tile in moveTiles)
            tile.SetHighlight(TileHighlightType.Move, true);

        // Destacar ataque cuerpo a cuerpo
        var meleeTiles = GridManager.Instance.GetTilesInRange(
            hero.OccupiedTile, hero.meleeAttackDistance, false);
        foreach (var tile in meleeTiles)
            tile.SetHighlight(TileHighlightType.MeleeAttack, true);

        // Destacar ataque a distancia
        var rangedTiles = GridManager.Instance.GetTilesInRange(
            hero.OccupiedTile, hero.rangeAttackDistance, false);
        foreach (var tile in rangedTiles)
            tile.SetHighlight(TileHighlightType.RangeAttack, true);
    }

    public bool AllHeroesHaveActed()
    {
        return _heroes.All(h => h != null && h.HasActed);
    }

    public void HandleEnemyTurn()
    {
        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        // Limpiar referencias destruidas
        _enemies = _enemies.Where(e => e != null).ToList();

        foreach (var enemy in _enemies)
        {
            if (enemy.HasActed) continue;

            Debug.Log($"[ENEMY TURN] {enemy.name} hasActed={enemy.HasActed}, velocity={enemy.velocity}");

            // Movimiento aleatorio
            for (int i = 0; i < enemy.velocity; i++)
            {
                var availableTiles = GridManager.Instance.GetTilesFor(enemy, enemy.velocity);
                if (availableTiles.Count == 0) break;

                var randomTile = availableTiles[Random.Range(0, availableTiles.Count)];
                enemy.OccupiedTile.SetUnit(null);
                randomTile.SetUnit(enemy);
                enemy.SetTile(randomTile);

                Debug.Log($"{enemy.name} moved to ({enemy.OccupiedTile.X}, {enemy.OccupiedTile.Y})");
                yield return new WaitForSeconds(0.25f);
            }

            // Atacar al héroe más cercano
            var aliveHeroes = _heroes.Where(h => h != null).ToList();
            if (aliveHeroes.Count == 0) break;

            var minDist = aliveHeroes.Min(h =>
                Vector2.Distance(h.transform.position, enemy.transform.position));

            var closest = aliveHeroes
                .Where(h => Mathf.Approximately(
                    Vector2.Distance(h.transform.position, enemy.transform.position), minDist))
                .ToList();

            var target = closest[Random.Range(0, closest.Count)];
            enemy.Attack(target);
            enemy.SetHasActed(true);

            yield return new WaitForSeconds(0.5f);
        }

        // Verificar victoria/derrota antes de resetear
        CheckGameOver();

        // 1) Resetear héroes
        foreach (var hero in _heroes.Where(h => h != null))
            hero.SetHasActed(false);

        // 2) Resetear enemigos
        foreach (var enemy in _enemies.Where(e => e != null))
            enemy.SetHasActed(false);

        // 3) Cambio a turno héroes
        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }

    public void OnUnitDeath(BaseUnit unit)
    {
        if (unit.Faction == Faction.Hero)
            _heroes.Remove(unit as BaseHero);
        else if (unit.Faction == Faction.Enemy)
            _enemies.Remove(unit as BaseEnemy);

        CheckGameOver();
    }

    public void CheckGameOver()
    {
        // Limpiar listas de referencias nulas
        _heroes = _heroes.Where(h => h != null).ToList();
        _enemies = _enemies.Where(e => e != null).ToList();

        if (_enemies.Count == 0)
        {
            MenuManager.Instance.ShowVictory();
            GameManager.Instance.ChangeState(GameState.None);
        }
        else if (_heroes.Count == 0)
        {
            MenuManager.Instance.ShowDefeat();
            GameManager.Instance.ChangeState(GameState.None);
        }
    }
}
