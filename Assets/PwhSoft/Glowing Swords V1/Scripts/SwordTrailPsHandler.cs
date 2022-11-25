using UnityEngine;

namespace PwhSoft.Glowing_Swords_V1.Scripts
{
    public class SwordTrailPsHandler : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem particleSystemTop;
        
        [SerializeField]
        private ParticleSystem particleSystemBottom;
        
        public void UpdateColor(Color color)
        {
            ChangeColorBySpeed(color, particleSystemTop);
            ChangeColorBySpeed(color, particleSystemBottom);
        }

        private static void ChangeColorBySpeed(Color color, ParticleSystem particleSystem)
        {
            if (!particleSystem)
                return;

            var colorBySpeed = particleSystem.colorBySpeed;
            if (colorBySpeed.enabled)
            {
                colorBySpeed.color = color;
            }
        }
    }
}
