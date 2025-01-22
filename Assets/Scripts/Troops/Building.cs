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

        #region Public Variables
        public Link CreateLink() => Instantiate(link, transform);
        public BuildingType GetBuildingType() => buildingType;
        public List<Link> GetMyLinks() => _myLinks;
        public string GetId() => id;
        #endregion
       
        
        

        public void SetBuildingLink(Link myLink)
        {
            _myLinks.Add(myLink);
        }
    }
}
