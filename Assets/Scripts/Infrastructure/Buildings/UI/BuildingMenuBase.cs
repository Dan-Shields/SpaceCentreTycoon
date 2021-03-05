using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Buildings.Menus
{
    public abstract class BuildingMenuBase : MonoBehaviour
    {
        public Button closeButton;

        protected GameObject _building;

        public void Show(GameObject building)
        {
            _building = building;
        }

        public void Hide()
        {

        }
    }
}
