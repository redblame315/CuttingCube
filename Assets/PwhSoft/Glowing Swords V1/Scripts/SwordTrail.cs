using UnityEngine;

namespace PwhSoft.Glowing_Swords_V1.Scripts
{
    public class SwordTrail : MonoBehaviour
    {
        /// <summary>
        /// Assign a trail template to this field to create the sword trail during the sword swing.
        /// </summary>
        [SerializeField]
        public GameObject vfxTrail;

        /// <summary>
        /// Custom spawn point. If not set, the container to which this script is attached will be used as spawn point for the vfx trail.
        /// </summary>
        [SerializeField] 
        public GameObject customSpawnPoint;

        /// <summary>
        /// When activated, the trail is automatically activated when a movement is made, if it was previously automatically deactivated.
        /// </summary>
        [SerializeField]
        public bool autoActivateTrail;

        private GameObject _currentTrailObject;
        private Vector3 _lastWorldPosition;
        public SwordTrailPsHandler SwordTrailPsHelper { get; private set; }

        private void Start()
        {
            InitSwordTrailHelper();
            ActivateTrail();
        }

        private void InitSwordTrailHelper()
        {
            SwordTrailPsHelper ??= gameObject.GetComponentInChildren<SwordTrailPsHandler>();
        }

        /// <summary>
        /// Used to activate the trail.
        /// </summary>
        public void ActivateTrail () {
            var spawnPoint = !customSpawnPoint ? gameObject : customSpawnPoint;
            if (!_currentTrailObject && vfxTrail)
            {
                _currentTrailObject = Instantiate (vfxTrail, spawnPoint.transform.position, Quaternion.identity); //TODO pooling
                _currentTrailObject.transform.SetParent (transform);
                _currentTrailObject.transform.localRotation = Quaternion.identity;
                _currentTrailObject.transform.localScale = Vector3.one;
            }
        
            if (_currentTrailObject)
                InitSwordTrailHelper();
        
            _currentTrailObject.SetActive(true);
        }

        public void FixedUpdate()
        {
            //Cancel if already activated
            if (_currentTrailObject)
                return;
        
            //Cancel if feature is deactivated
            if (!autoActivateTrail)
                return;
        
            var currentWorldPos = gameObject.transform.position;
            var difference = Mathf.Abs(Vector3.Distance(currentWorldPos, _lastWorldPosition));
            if (_lastWorldPosition == Vector3.zero || difference > 0.01f)
            {
                ActivateTrail();
                _lastWorldPosition = currentWorldPos;            
            }
        }

        public void DeactivateTrail()
        {
            if (_currentTrailObject)
                _currentTrailObject.SetActive(false);
        }

        public void UpdateColor(Color color)
        {
            if (ReferenceEquals(SwordTrailPsHelper, null))
                return;
            SwordTrailPsHelper.UpdateColor(color);
        }
    }
}
