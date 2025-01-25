using System.Collections.Generic;
using UnityEngine;

namespace Troops
{
    public enum BuildingType
    {
        Troops, Weapon, Buff
    }
    
    public class Building : TroopParent
    {
        [SerializeField] private BuildingType buildingType;
        [SerializeField] private string id;
        [SerializeField] private Link link;
        [SerializeField] private int linksCount = 1;
        private List<Link> _myLinks = new List<Link>();
        private bool _active;
        private bool _linkedToTroops;


        protected override void SetBuilding()
        {
            SetActive(buildingType == BuildingType.Troops);
        }
        
        #region Public Variables
        public Link CreateLink() => Instantiate(link, transform);
        public BuildingType GetBuildingType() => buildingType;
        public List<Link> GetMyLinks() => _myLinks;
        public string GetId() => id;
        public bool IsActive() => _active;
        #endregion
        
        #region Links
        private void LinksUpdated()
        {
            
        }
        public void SetBuildingLink(Link myLink)
        {
            _myLinks.Add(myLink);
            var active = false;
            foreach (var l in _myLinks)
            { 
                if(l.LinkToActiveBuilding()) active = true;
            }
            
            if(!active) return;
            foreach (var l in _myLinks)
            { 
                l.SetAllLinkedBuildingsActive();
            }
        }
        public void RemoveLink(Link myLink)
        {
            _myLinks.Remove(myLink);
        }
        #endregion
        
        #region Visuals
        public void Highlite()
        {
            foreach (var r in GetRenderers())
            {
                float highlightValue = (Mathf.Sin(Time.time * Mathf.PI) + 1) / 2;
                r.material.SetFloat("_Highlite", highlightValue);
            }
        }
        public void RemoveHighlite()
        {
            foreach (var r in GetRenderers())
            {
                r.material.SetFloat("_Highlite", 0);
            }
        }
        public void SetActive(bool active)
        {
            _active = active;
            foreach (var r in GetRenderers())
            {
                r.material.SetFloat("_Saturation", active ? 1 : 0);
            }
        }
        #endregion
        
        
    }
}
