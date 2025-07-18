using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Tile;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour {
    public static GridManager Instance;
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _grassTile, _mountainTile;

    [SerializeField] private Transform _cam;

    private Dictionary<Vector2, Tile> _tiles;

    void Awake() {
        Instance = this;
    }

    public void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++) {
                var randomTile = Random.Range(0, 6) == 3 ? _mountainTile : _grassTile;
                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

              
                spawnedTile.Init(x,y);


                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);

        GameManager.Instance.ChangeState(GameState.SpawnHeroes);
    }

    public Tile GetHeroSpawnTile() {
        return _tiles.Where(t => t.Key.x < _width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public Tile GetEnemySpawnTile()
    {
        return _tiles.Where(t => t.Key.x > _width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }

    public List<Tile> GetTilesInRange(Tile startTile, int range, bool requireWalkable)
    {
        List<Tile> results = new List<Tile>();
        Vector2 startPos = startTile.transform.position;

        foreach (var pair in _tiles)
        {
            Vector2 pos = pair.Key;
            Tile tile = pair.Value;

            int distance = Mathf.Abs((int)(pos.x - startPos.x)) + Mathf.Abs((int)(pos.y - startPos.y));
            if (distance <= range)
            {
                if (requireWalkable && !tile.Walkable) continue;
                results.Add(tile);
            }
        }

        return results;
    }

    public int GetDistance(Tile a, Tile b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    public List<Tile> GetTilesFor(BaseUnit unit, int range)
    {
        return GetTilesInRange(unit.OccupiedTile, range, true);
    }

    public void ClearAllHighlights()
    {
        foreach (var tile in _tiles.Values)
        {
            tile.SetHighlight(TileHighlightType.Move, false);
            tile.SetHighlight(TileHighlightType.RangeAttack, false);
            tile.SetHighlight(TileHighlightType.MeleeAttack, false);
            tile.SetHighlight(TileHighlightType.Hover, false);
        }
    }
}