using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] public BoundsInt _area;

    [SerializeField] private TileBase _backgroundTile;

    [SerializeField] private TileBase _selectionTile;
    [SerializeField] private TileBase _preDestroyTile;
    [SerializeField] private TileBase _destroyTile;
    [SerializeField] private TileBase _createTile;

    [SerializeField] private TileBase _panelTile;

    public Tilemap _previewTilemap;
    public Tilemap _panelTilemap;
    public Tilemap _backgroundTilemap;

    public TMP_Text _buildToggleText;
    public GameObject _buildToggle;

    public TMP_Text _moveToggleText;
    public GameObject _moveToggle;

    public TMP_Text _addZoneToggleText;
    public GameObject _addZoneToggle;

    public TMP_Text _removeZoneToggleText;
    public GameObject _removeZoneToggle;
    public bool zoneMade;
    public int zonePanelsAssigned = 0;
    public List<GameObject> zoneList = new List<GameObject>();

    public GameObject _continueButton;

    public GameObject _line;
    private GameObject lineRender;

    private Vector3Int previous;

    private ColorBlock cb;
    private ColorBlock cb2;



    public Text _stage;

    private Vector3Int lastSelected = new Vector3Int(0, 0, -10);
    public bool dragging = false;

    private Vector3Int moveOrigin;
    private bool legalMove = false;
    private bool moveStarted = false;

    public string mode = "build";
    public string lastMode = "build";

    private bool legal;

    private List<Vector3Int> points = new List<Vector3Int>();

    public enum AppState { Placement, Zoning }
    public AppState state = AppState.Placement;

    private void Awake() {
        cb = _buildToggle.GetComponent<Button>().colors;
        cb2 = _buildToggle.GetComponent<Button>().colors;
        cb.normalColor = new Color(0.1933517f, 0.745283f, 0.3098039f, 1f);
        cb.highlightedColor = new Color(0.1933517f, 0.745283f, 0.3098039f, 1f);
        cb.pressedColor = new Color(0.1141421f, 0.5377358f, 0.2042684f, 1f);
        cb.selectedColor = new Color(0.1933517f, 0.745283f, 0.3098039f, 1f);
    }

    private void Update() {
        switch(state) {
            case AppState.Placement:
                HandlePlacement();
                break;
            case AppState.Zoning:
                HandleZoning();
                break;
        }
    }

    private void HandlePlacement() {

        if(Input.GetMouseButtonDown(0)) {
            dragging = true;
        }

        if(Input.GetMouseButtonUp(0)) {
            dragging = false;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int selectedTile = _previewTilemap.WorldToCell(mousePos);

        selectedTile.z = 0;

        TileBase currentType = _panelTilemap.GetTile(selectedTile);
        if(selectedTile != lastSelected) {
            _previewTilemap.SetTile(lastSelected, null);
            if(_backgroundTilemap.GetTile(new Vector3Int(selectedTile.x, selectedTile.y, 0)) == _backgroundTile) {
                if(currentType == null) {
                    if(mode == "build") {
                        legal = true;
                        Vector3Int[] neighbors = GetNeighborsInRange(selectedTile, 1);
                        foreach(Vector3Int neighbor in neighbors) {
                            if(_panelTilemap.GetTile(neighbor) == _panelTile || _panelTilemap.GetTile(neighbor) == _createTile) {
                                legal = true;
                            }
                        }
                        if (legal) {
                            _previewTilemap.SetTile(selectedTile, _selectionTile);
                        } else {
                            _previewTilemap.SetTile(selectedTile, _preDestroyTile);
                        }
                    }
                } else if(mode == "destroy") {
                    _previewTilemap.SetTile(selectedTile, _preDestroyTile);
                }
            }
            lastSelected = selectedTile;
        }
        if(dragging && legal && (_backgroundTilemap.GetTile(new Vector3Int(selectedTile.x, selectedTile.y, 0)) == _backgroundTile)) {
            if(currentType == null || currentType == _createTile) {
                if(mode == "build") {
                    _panelTilemap.SetTile(selectedTile, _createTile);
                }
                
            } else {
               if(mode == "destroy") {
                    _panelTilemap.SetTile(selectedTile, _destroyTile);
                }
            }
            _previewTilemap.SetTile(selectedTile, null);
        } else {
            dragging = false;
            _panelTilemap.SwapTile(_destroyTile, null);
            _panelTilemap.SwapTile(_createTile, _panelTile);
        }

        if(Input.GetMouseButtonDown(1)) {
            if(mode != "move") {
                ToggleBuild();
            }
        }

        if(mode == "move") {

            if(Input.GetMouseButtonDown(0) && currentType == _panelTile) {
                moveOrigin = selectedTile;
                moveStarted = true;
                _panelTilemap.SetTile(moveOrigin, _preDestroyTile);
                lineRender = Instantiate(_line, moveOrigin, Quaternion.identity);
            }

            if(moveStarted) {
                legalMove = true;
                Vector3Int[] neighbors = GetNeighborsInRange(selectedTile, 1);
                foreach(Vector3Int neighbor in neighbors) {
                    if(_panelTilemap.GetTile(neighbor) == _panelTile) {
                        legalMove = true;
                    }
                }
                if(_panelTilemap.GetTile(selectedTile) != null) {
                    legalMove = false;
                }
                if(legalMove) {
                    _panelTilemap.SetTile(moveOrigin, _destroyTile);
                    _previewTilemap.SetTile(selectedTile, _createTile);
                    lineRender.GetComponent<LineRenderer>().startColor = new Color((188f/255), (188f/255), (188f/255), 1f);
                    lineRender.GetComponent<LineRenderer>().endColor = new Color((188f/255), (188f/255), (188f/255), 1f);
                    
                } else {
                    _panelTilemap.SetTile(moveOrigin, _preDestroyTile);
                    _previewTilemap.SetTile(selectedTile, _preDestroyTile);
                    lineRender.GetComponent<LineRenderer>().startColor = new Color(1f, (100f/255), (100f/255), 1f);
                    lineRender.GetComponent<LineRenderer>().endColor = new Color(1f, (100f/255), (100f/255), 1f);
                }

                lineRender.GetComponent<LineRenderer>().SetPosition(0, _previewTilemap.CellToWorld(moveOrigin));
                lineRender.GetComponent<LineRenderer>().SetPosition(1, _previewTilemap.CellToWorld(selectedTile));

            }

            if(Input.GetMouseButtonUp(0)) {
                if(legalMove && moveStarted) {
                    _panelTilemap.SetTile(moveOrigin, null);
                    _panelTilemap.SetTile(selectedTile, _panelTile);
                } else if(moveStarted) {
                    _panelTilemap.SetTile(moveOrigin, _panelTile);
                }
                moveStarted = false;
                Destroy(lineRender);
                _previewTilemap.SetTile(selectedTile, null);
            }

            if(Input.GetMouseButtonDown(1)) {
                _panelTilemap.SetTile(moveOrigin, _panelTile);
                moveStarted = false;
                Destroy(lineRender);
                _previewTilemap.SetTile(selectedTile, null);
            }
        }
    }

    public int FindPanelNum() {
        int amount = 0;
        TileBase[] allPanels = _panelTilemap.GetTilesBlock(_panelTilemap.cellBounds);

        foreach(TileBase tile in allPanels) {
            if(tile == _panelTile){
                amount++;
            }
        }

        return amount;
    }

    public Vector3Int[] GetNeighborsInRange(Vector3Int pos, int range) {

        Vector3Int centerCubePos = GetVector3Coord(pos);

        Vector3Int[] tiles = new Vector3Int[6];
        int i = 0;

        int min = -range, max = range;

        for (int x = min; x <= max; x++) {

            for(int y = min; y <= max; y++) {

                for(int z = min; z <= max; z++) {

                    if (x + y + z == 0 && (x != 0 || y != 0 || z != 0)) {

                        Vector3Int cubePosOffset = new Vector3Int(x, y, z);
                        Vector3Int newTilePos = GetVector2Coord(centerCubePos + cubePosOffset);
                        tiles[i] = newTilePos;
                        i++;
                    }

                }

            }

        }

        return tiles;

    }

    public Vector3Int GetVector2Coord(Vector3Int pos) {

        int x = (int) pos.x, z = (int) pos.z;
        int newX = x, newZ = z + (x - (x&1)) / 2;
        return new Vector3Int(newZ, newX, 0);

    }

    public Vector3Int GetVector3Coord(Vector3Int pos) {

        int x = (int) pos.y, y = (int) pos.x;
        int tileX = x, tileZ = y - (x - (x&1)) / 2, tileY = -tileX - tileZ;
        return new Vector3Int(tileX, tileY, tileZ);

    }

    // TOGGLES

    public void ToggleBuild() {
        if(mode == "build") {
            mode = "destroy";
            _buildToggleText.GetComponent<TMP_Text>().text = "Add Panels";
            _buildToggle.GetComponent<Button>().colors = cb;
        } else if(mode == "destroy") {
            mode = "build";
            _buildToggleText.GetComponent<TMP_Text>().text = "Remove Panels";
            _buildToggle.GetComponent<Button>().colors = cb2;
        }
    }

    public void ToggleMove() {
        if(mode != "move") {
            lastMode = mode;
            mode = "move";
            _moveToggleText.GetComponent<TMP_Text>().text = "Stop Moving";
            _buildToggle.GetComponent<Button>().interactable = false;
        } else {
            mode = lastMode;
            _moveToggleText.GetComponent<TMP_Text>().text = "Move Panels";
            _buildToggle.GetComponent<Button>().interactable = true;
        }
    }

    public void ToggleContinue() {
        if(state == AppState.Placement) {
            state = AppState.Zoning;
            _buildToggle.GetComponent<Button>().interactable = false;
            _moveToggle.GetComponent<Button>().interactable = false;
            _continueButton.GetComponent<Button>().interactable = false;
            _stage.text = "Zoning Stage";
            mode = "draw";

            _buildToggle.SetActive(false);
            _moveToggle.SetActive(false);
            _addZoneToggle.SetActive(true);
            _removeZoneToggle.SetActive(true);
        }
    }

    public void AddZone() {
        if (lineRender != null) {
            lineRender.GetComponent<LineRenderer>().startColor = new Color((219f/255f), (124f/255f), (59f/255f), 1f);
            lineRender.GetComponent<LineRenderer>().endColor = new Color((219f/255f), (124f/255f), (59f/255f), 1f);
            zoneList.Add(lineRender);
            zoneMade = true;
            zonePanelsAssigned += lineRender.GetComponent<LineRenderer>().positionCount;
            _addZoneToggle.GetComponent<Button>().interactable = false;

            if(FindPanelNum() == zonePanelsAssigned) {
                foreach (GameObject zone in zoneList) {
                    zone.GetComponent<LineRenderer>().startColor = new Color((100f/255f), 1f, (100f/255f), 1f);
                    zone.GetComponent<LineRenderer>().endColor = new Color((100f/255f), 1f, (100f/255f), 1f);
                }
                _continueButton.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void RemoveZone() {
        mode = "remove";
        _removeZoneToggle.GetComponent<Button>().interactable = false;
    }

    public void RemoveZonePoints(LineRenderer line) {
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        foreach (var position in positions) {
            Vector3Int positionInt = _previewTilemap.WorldToCell(position);
            if (points.Contains(positionInt)) {
                points.Remove(positionInt);
            }
        }
    }

    private void HandleZoning() {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int selectedTile = _previewTilemap.WorldToCell(mousePos);

        selectedTile.z = 0;

        TileBase currentType = _panelTilemap.GetTile(selectedTile);
        if(Input.GetMouseButtonDown(0) && currentType == _panelTile && !points.Contains(selectedTile)) {
            moveStarted = true;
            if(lineRender != null) {
                if(zoneMade) {
                    zoneMade = false;
                } else {
                    RemoveZonePoints(lineRender.GetComponent<LineRenderer>());
                    Destroy(lineRender);
                }
            }
            lineRender = Instantiate(_line, selectedTile, Quaternion.identity);
            lineRender.GetComponent<LineRenderer>().positionCount = 1;
            points.Add(selectedTile);
            moveOrigin = selectedTile;

            lineRender.GetComponent<LineRenderer>().startColor = new Color((100f/255f), (100f/255f), 1f, 1f);
            lineRender.GetComponent<LineRenderer>().endColor = new Color((100f/255f), (100f/255f), 1f, 1f);
        }
        if(selectedTile != lastSelected) {
            lastSelected = selectedTile;
            Vector3Int[] neighbors = GetNeighborsInRange(selectedTile, 1);
            legalMove = false;
            foreach (Vector3Int neighbor in neighbors) {
                if (neighbor == moveOrigin) {
                    legalMove = true;
                }
            }
            if (points.Contains(selectedTile)) {
                if (points.IndexOf(selectedTile) < zonePanelsAssigned) {
                    legalMove = false;
                }
            }

            if (moveStarted && currentType == _panelTile && legalMove) {
                if (!points.Contains(selectedTile)) {
                    points.Add(selectedTile);
                } else if (points.IndexOf(selectedTile) >= zonePanelsAssigned) {
                    points.RemoveRange(points.IndexOf(selectedTile) + 1, lineRender.GetComponent<LineRenderer>().positionCount - points.IndexOf(selectedTile) - 1);
                }
                moveOrigin = selectedTile;

                lineRender.GetComponent<LineRenderer>().positionCount = points.Count - zonePanelsAssigned;
                for (int i = zonePanelsAssigned; i < points.Count; i++) {
                    lineRender.GetComponent<LineRenderer>().SetPosition(i-zonePanelsAssigned, _previewTilemap.CellToWorld(points[i]));
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && moveStarted) {
            moveStarted = false;
            _addZoneToggle.GetComponent<Button>().interactable = true;
            if (mode == "draw") {
                lineRender.GetComponent<LineRenderer>().startColor = new Color(1f, (100f/255f), (100f/255f), 1f);
                lineRender.GetComponent<LineRenderer>().endColor = new Color(1f, (100f/255f), (100f/255f), 1f);
                _continueButton.GetComponent<Button>().interactable = false;
            }
        }

        if (mode == "remove") {
            if (Input.GetMouseButtonDown(0)) {
                foreach (GameObject zone in zoneList) {
                    if (PointInZone(zone.GetComponent<LineRenderer>(), selectedTile)) {
                        RemoveZonePoints(zone.GetComponent<LineRenderer>());
                        zonePanelsAssigned -= zone.GetComponent<LineRenderer>().positionCount;
                        zoneList.Remove(zone);
                        Destroy(zone);
                        mode = "draw";
                        foreach (GameObject line in zoneList) {
                            line.GetComponent<LineRenderer>().startColor = new Color((219f/255f), (124f/255f), (59f/255f), 1f);
                            line.GetComponent<LineRenderer>().endColor = new Color((219f/255f), (124f/255f), (59f/255f), 1f);
                        }
                        _removeZoneToggle.GetComponent<Button>().interactable = true;
                        _continueButton.GetComponent<Button>().interactable = false;
                        break;
                    }
                }
            }
        }
    }

    private bool PointInZone(LineRenderer line, Vector3Int point) {
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        foreach (var position in positions) {
            Vector3Int positionInt = _previewTilemap.WorldToCell(position);
            if (point == positionInt) {
                return true;
            }
        }
        return false;
    }
}