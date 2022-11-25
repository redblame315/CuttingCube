using System.Collections.Generic;
using PwhSoft.Additional_Assets.Scripts;
using PwhSoft.Glowing_Swords_V1.Scripts;
using UnityEngine;

namespace PwhSoft.Glowing_Swords_V1.Demo
{
    public class GameManager : MonoBehaviour
    {
        private List<GlowingSword> _lightSabers;
        
        // Start is called before the first frame update
        private void Start()
        {
            _lightSabers = gameObject.FindChildrenByType<GlowingSword>();
        }

        public void ToggleLightSabers()
        {
            _lightSabers?.ForEach(s=>s.ToggleActive());
        }

        public void ToggleLightSabersTrails()
        {
            _lightSabers?.ForEach(s=>s.ToggleActiveTrails());
        }
    }
}
