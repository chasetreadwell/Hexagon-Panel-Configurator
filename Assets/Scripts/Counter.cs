using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public GridManager _gridManager;
    public GameObject _txt;

    public GameObject _continueButton;

    private int prevPanels;
    private int prevZones = -1;

    private bool canContinue = false;

    private void Update() {
        if (_gridManager.state == GridManager.AppState.Placement) {
            PlacementUpdate();
        } else if (_gridManager.state == GridManager.AppState.Zoning) {
            ZoningUpdate();
        }
    }

    private void PlacementUpdate() {
        if(!_gridManager.dragging) {
            int totalPanels = _gridManager.FindPanelNum();
            if(totalPanels != prevPanels) {
                _txt.GetComponent<Text>().text = "Panels: " + totalPanels.ToString();

                prevPanels = totalPanels;
                
                if(totalPanels > 32 || totalPanels < 8) {
                    _txt.GetComponent<Text>().color = new Color(1f, (100f/255), (100f/255), 1f);
                    if(canContinue) {
                        canContinue = false;
                        _continueButton.GetComponent<Button>().interactable = false;
                    }
                } else {
                    _txt.GetComponent<Text>().color = Color.white;
                    if(!canContinue) {
                        canContinue = true;
                        _continueButton.GetComponent<Button>().interactable = true;
                    }
                }
            }
        }
    }

    private void ZoningUpdate() {
        if(!_gridManager.dragging) {
            int totalZones =_gridManager.zoneList.Count;
            if(totalZones != prevZones) {
                _txt.GetComponent<Text>().text = "Zones: " + totalZones.ToString();
                prevZones = totalZones;
            }
        }
    }
}
