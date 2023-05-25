using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    void Start()
    {
        HighlightTile(false);
    }
    private Vector2Int coords;
    private List<Tile> Neighbours = new List<Tile>();
    private MaterialPropertyBlock block;
    private Color currColor;
    private static Color defaultColor = Color.grey;
    public static Color terrainColor = Color.black;
    public static Color roomColor = Color.white;
    public static Color edgeColor = Color.black;
    public static Color corridorColor = Color.white;
    public static float defaultVarience = 0;
    public static float terrainVarience = 0;
    public static float roomVarience = 0;
    public static float edgeVarience = 0;
    public static float corridorVarience = 0;
    private Room room;
    public enum TileType
    {
        blank,
        terrain,
        room,
        Edge,
        hidden,
        Corridor
    }
    public TileType type = TileType.blank;
    public static void SetColor(int _typeIndex, float _varience = 0)
    {
        Color col = ColorPickerControl.instance.GetColor(); //Get col from color picker
        switch((TileType)_typeIndex)
        {
            case TileType.blank:
                defaultColor = col;
                defaultVarience = _varience;
                break;
            case TileType.terrain:
                terrainColor = col;
                terrainVarience = _varience;
                break;
            case TileType.room:
                roomColor = col;
                roomVarience = _varience;
                break;
            case TileType.Edge:
                edgeColor = col;
                edgeVarience = _varience;
                break;
            case TileType.hidden:
                break;
            case TileType.Corridor:
                corridorColor = col;
                corridorVarience = _varience;
                break;
            default:
                break;
        }
        TileGrid.instance.RefreshTileColors();
    }
    public static Color GetColor(int _typeIndex)
    {
        switch((TileType)_typeIndex)
        {
            case TileType.blank:
                return defaultColor;
            case TileType.terrain:
                return terrainColor;
            case TileType.room:
                return roomColor;
            case TileType.Edge:
                return edgeColor;
            case TileType.hidden:
                return defaultColor;
            case TileType.Corridor:
                return corridorColor;
            default:
                return defaultColor;
                break;
        }
    }
    public bool insideRoom()
    {
        return room == null ? false : true;
    }
    public void Init(Vector2Int _coords)
    {
        coords = _coords;
    }
    public static float GetDistance(Tile a,Tile b, TileGrid.TileGridShape _shape)
    {
        if(_shape == TileGrid.TileGridShape.SQUARE)
        {
            return Vector2.Distance(a.coords,b.coords);
        }
        //Figure out if either tile is offset
        Vector2 aHexCoord,bHexCoord;
        aHexCoord = new Vector2(a.transform.position.x,a.transform.position.z);
        bHexCoord = new Vector2(b.transform.position.x,b.transform.position.z);
        return Vector2.Distance(aHexCoord,bHexCoord);
    }
    public List<Tile> GetNeighbours(bool _includeAll = false, int _radius = 1)
    {
        Neighbours = new List<Tile>();
        Vector2Int searchCorner = new Vector2Int(coords.x - _radius, coords.y - _radius);
        int diameter = (_radius * 2) + 1;
        for(int i = 0; i < diameter; i++){
            for(int j = 0; j < diameter; j++)
            {
                Vector2Int coord;
                if(TileGrid.instance.GetTileGridShape() == TileGrid.TileGridShape.SQUARE)
                {
                    coord = searchCorner + new Vector2Int(i,j);
                }
                else
                {
                    if(j % 2 == 0)//Even
                    {
                        coord = coord = searchCorner + new Vector2Int(i,j);
                    }   
                    else//Odd
                    {
                        coord = searchCorner + new Vector2Int(i - 1, j);
                    }
                }
                Tile neighbourTile = TileGrid.instance.GetTile(coord);
                if(neighbourTile == null){continue;}
                if(neighbourTile.type != TileType.blank && !_includeAll){continue;}
                if(neighbourTile == this){continue;}
                Neighbours.Add(neighbourTile);
            }
        }
        return Neighbours;
        // switch(TileGrid.instance.GetTileGridShape())
        // {
        //     case TileGrid.TileGridShape.SQUARE:
        //         AddNeighbourToList(new Vector2Int(-1,-1),_includeAll);
        //         AddNeighbourToList( new Vector2Int(-1,0),_includeAll);
        //         AddNeighbourToList( new Vector2Int(-1,1),_includeAll);

        //         AddNeighbourToList(new Vector2Int(0,-1),_includeAll);
        //         AddNeighbourToList( new Vector2Int(0,1),_includeAll);

        //         AddNeighbourToList( new Vector2Int(1,-1),_includeAll);
        //         AddNeighbourToList( new Vector2Int(1,0),_includeAll);
        //         AddNeighbourToList( new Vector2Int(1,1),_includeAll);
        //         break;
        //     case TileGrid.TileGridShape.HEX:
        //         AddNeighbourToList(new Vector2Int(-1,0),_includeAll);
        //         AddNeighbourToList(new Vector2Int(1,0),_includeAll);

        //         AddNeighbourToList(new Vector2Int(0,-1),_includeAll);
        //         AddNeighbourToList(new Vector2Int(0,1),_includeAll);
        //         if(coords.y % 2 == 0)
        //         {
        //             AddNeighbourToList(new Vector2Int(1,-1),_includeAll);
        //             AddNeighbourToList(new Vector2Int(1,1),_includeAll);
        //         }
        //         else
        //         {
        //             AddNeighbourToList(new Vector2Int(-1,1),_includeAll);
        //             AddNeighbourToList(new Vector2Int(-1,-1),_includeAll);
        //         }
        //         break;
        //     default:
        //         break;
        // }
        // return Neighbours;
        //Prune list
    }
    private void AddNeighbourToList(Vector2Int offset, bool _includeAll = false)
    {
        Vector2Int coord = coords + offset;
        Tile newTile = TileGrid.instance.GetTile(coord);
        if(newTile == null){return;}
        if(!TileGrid.instance.highlightTerrain && newTile.type == TileType.terrain && !_includeAll){return;}
        if(Neighbours.Contains(newTile)){return;}
        Neighbours.Add(newTile);
    }
    public void HighlightTile(bool _highLight)
    {
        block = new MaterialPropertyBlock();
        this.GetComponent<Renderer>().GetPropertyBlock(block);
        Color col = _highLight ? Color.white : currColor;
        block.SetColor("_BaseColor", col);
        this.GetComponent<Renderer>().SetPropertyBlock(block);
    }
    void OnMouseDown()
    {
        if(Input.GetMouseButton(0))
        {
            if(TileGrid.instance.highlightTerrain || type != TileType.terrain)
            {
                HighlightTile(true);
            }
            foreach(Tile tile in TileGrid.instance.GetTilesWithinDistance(coords,TileGrid.instance.highlightRadius))
            {
                tile.HighlightTile(true);
            }
        }
    }
    void OnMouseUp()
    {
        foreach(Tile tile in TileGrid.instance.GetTilesWithinDistance(coords,TileGrid.instance.highlightRadius))
        {
            tile.HighlightTile(false);
        }
        HighlightTile(false);
    }
    public void PaintTile(TileType _type, bool _overrideAll = false)
    {
        if(type == TileType.Edge && _overrideAll == false){return;}
        this.GetComponent<Renderer>().enabled = true;
        Vector3 pos = transform.position;
        Color col = defaultColor;
        float varience = 0;
        switch(_type)
        {
            case TileType.blank:
                pos.y = 0;
                col = defaultColor;
                varience = defaultVarience;
                break;
            case TileType.terrain:
                pos.y = 0;
                col = terrainColor;
                varience = terrainVarience;
                break;
            case TileType.room:
                pos.y = .0f;
                col = roomColor;
                varience = roomVarience;
                break;
            case TileType.Edge:
                pos.y = 1;
                col = edgeColor;
                varience = edgeVarience;
                break;
            case TileType.hidden:
                pos.y = 0;
                this.GetComponent<Renderer>().enabled = false;
                break;
            case TileType.Corridor:
                pos.y = 0.0f;
                col = corridorColor;
                varience = corridorVarience;
                break;
        }
        if(varience != 0)
        {
            col *= Random.Range(1 - varience, 1 + varience);
        }
        transform.position = pos;
        type = _type;
        block = new MaterialPropertyBlock();
        this.GetComponent<Renderer>().GetPropertyBlock(block);
        block.SetColor("_BaseColor", col);
        currColor = col;
        this.GetComponent<Renderer>().SetPropertyBlock(block);
    }
    public void TempSetColor(Color col)
    {
        block = new MaterialPropertyBlock();
        this.GetComponent<Renderer>().GetPropertyBlock(block);
        block.SetColor("_BaseColor", col);
        this.GetComponent<Renderer>().SetPropertyBlock(block);
    }
    public void ResetColor()
    {
        this.GetComponent<Renderer>().enabled = true;
        //Check if edge
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;
        type = TileType.blank;
        block = new MaterialPropertyBlock();
        this.GetComponent<Renderer>().GetPropertyBlock(block);
        block.SetColor("_BaseColor", defaultColor);
        currColor = defaultColor;
        this.GetComponent<Renderer>().SetPropertyBlock(block);
    }
    public Vector2Int GetCoords()
    {
        return coords;
    }
}

