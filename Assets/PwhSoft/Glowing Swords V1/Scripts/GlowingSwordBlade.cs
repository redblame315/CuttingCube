using System;
using System.Linq;
using UnityEngine;

namespace PwhSoft.Glowing_Swords_V1.Scripts
{
    /// <summary>
    /// Used to animate the switching on and off of the blade or to change the color of the blade.
    /// </summary>
    public class GlowingSwordBlade : MonoBehaviour
    {
        #region Private Members

        private bool _animationRunning;
        
        private Color _color = Color.red;
        
        // the light attached to the blade
        private Light _light;


        /// <summary>
        /// Condition whether the blade is activated or deactivated.
        /// </summary>
        private bool _bladeActive;

        /// <summary>
        /// The delta is a Lep value within 1 second. 
        /// It is initialized depending on the blade speed.
        /// </summary>
        private float _extendDelta;

        /// <summary>
        /// The local x scale of the blade.
        /// </summary>
        private float _localXScale;
        
        /// <summary>
        /// The local z scale of the blade.
        /// </summary>
        private float _localZScale;
        
        /// <summary>
        /// The min blade scale.
        /// </summary>
        private float _minScale;

        /// <summary>
        /// The max scale of the blade.
        /// </summary>
        private float _maxScale;

        /// <summary>
        /// The current scale of the blade.
        /// </summary>
        private float _scaleCurrent;

        /// <summary>
        /// Shader property index of the color.
        /// </summary>
        private static readonly int ColorPropertyIndex = Shader.PropertyToID("_Color");
        
        #endregion
        
        #region Mesh Renderer

        private MeshRenderer _meshRenderer;
        private SwordTrail _swordBladeTrail;

        private MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                    _meshRenderer = gameObject.GetComponent<MeshRenderer>();

                if (_meshRenderer != null) return _meshRenderer;
                
                Debug.LogError("No mesh renderer found.");
                return null;
            }
        }

        #endregion
    
        #region Properties 

        /// <summary>
        /// Property, to activate or deactivate the blade.
        /// </summary>
        public bool BladeActive
        {
            get => _bladeActive;
            set
            {
                if (_bladeActive.Equals(value))
                    return;

                _bladeActive = value;
                _extendDelta = _bladeActive ? Mathf.Abs(_extendDelta) : -Mathf.Abs(_extendDelta);
                
                
                if (!Application.isPlaying && Application.isEditor)
                    ForceSetActive(_bladeActive);
                else 
                    _animationRunning = true;
            }
        }
        
        /// <summary>
        /// Property for the color of the blade.
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {   
                _color = value;
                
                UpdateLight();
                UpdateTrail();

                var materials = GetMaterials();
                if (materials[0] == null)
                {
                    Debug.LogError("No material found. Please attach the glowing sword blade material.");
                    return;
                }

                materials[0].SetColor("_Color", _color);
            }
        }

        #endregion

        #region Setup

        /// <summary>
        /// Setup for the light saber blade.
        /// </summary>
        /// <param name="extendSpeed">The extend speed of the blade.</param>
        /// <param name="active">Activation status of the blade.</param>
        public void Setup(float extendSpeed, bool active)
        {
            _swordBladeTrail = gameObject.GetComponentInChildren<SwordTrail>();
            var materials = GetMaterials();
            var material = materials.FirstOrDefault();
            if (material == null)
            {
                Debug.LogError("No material was set to glowing sword blade. Assign a blade material to the blade.");
                return;
            }
            materials[0] = new Material(material);
            
            _light = gameObject.GetComponentInChildren<Light>();
            _bladeActive = active;

            // consistency check
            if (_light == null)
            {
                Debug.LogWarning(new NullReferenceException(nameof(_light) + " not found."));
                return;
            }

            // remember initial scale values (non extending part of the blade)
            var localScale = gameObject.transform.localScale;
            
            _localXScale = localScale.x;
            _localZScale = localScale.z;

            // remember initial scale values (extending part of the blade)
            _minScale = 0f;
            _maxScale = localScale.y;

            // initialize variables
            // the delta is a lerp value within 1 second. depending on the extend speed we have to size it accordingly
            _extendDelta = _maxScale / extendSpeed;
            
            _extendDelta = active ? Mathf.Abs(_extendDelta) : -Mathf.Abs(_extendDelta);
            
            ForceSetActive(active);
        }

        #endregion

        #region Updates
        
        /// <summary>
        /// Force deactivate the blade
        /// </summary>
        /// <param name="active"></param>
        private void ForceSetActive(bool active)
        {
            UpdateBladeScaleHeight(active?1f:0f);
            gameObject.SetActive(active);
        }

        private void Update()
        {
            if (_animationRunning)
            {
                UpdateSaberSize();
            }
        }
        
        private void UpdateBladeScaleHeight(float deltaScale)
        {
            _scaleCurrent = deltaScale;
            gameObject.transform.localScale = new Vector3(_localXScale, _scaleCurrent, _localZScale);
        }
  
        
        private void UpdateLight()
        {
            if (ReferenceEquals(_light, null)) 
                return;
            
            _light.color = _color;
        }
        
        private void UpdateTrail()
        {
            // ReSharper disable once UseNullPropagation
            if (ReferenceEquals(_swordBladeTrail, null))
                return;

            _swordBladeTrail.UpdateColor(_color);
        }

        private Func<bool> _funcTrailFeatureActivated;
        public void SetFuncTrailFeatureActivated(Func<bool> func) => _funcTrailFeatureActivated = func;
        
        public void UpdateSaberSize()
        {
            // consider delta time with blade extension
            _scaleCurrent += _extendDelta * Time.deltaTime;

            // clamp blade size
            _scaleCurrent = Mathf.Clamp(_scaleCurrent, _minScale, _maxScale);

            // scale in z direction
            UpdateBladeScaleHeight(_scaleCurrent);

            // whether the blade is active or not
            _bladeActive = _scaleCurrent > 0;
             
            // show / hide the gameobject depending on the blade active state
            if (_bladeActive && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            else if(!_bladeActive && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                _animationRunning = false;
                
                if (!_bladeActive)
                    ActivateTrail(false);
            }

            if (_animationRunning && _scaleCurrent >= 1f)
            {
                _animationRunning = false;
                OnAfterSaberActivated();
            }
        }

        private void OnAfterSaberActivated()
        {
            ActivateTrail(true);
        }

        public void ActivateTrail(bool activate)
        {
            if (activate && _funcTrailFeatureActivated != null && !_funcTrailFeatureActivated())
                return;
            
            // ReSharper disable once UseNullPropagation
            if (ReferenceEquals(_swordBladeTrail, null))
                return;
            
            if (activate)
                _swordBladeTrail.ActivateTrail();
            else
                _swordBladeTrail.DeactivateTrail();
        }

        /// <summary>
        /// Updates the lighting of the blade.
        /// </summary>
        public void UpdateLighting()
        {
            if (_light == null)
            {
                Debug.LogWarning($"{nameof(GlowingSword)}.{nameof(UpdateLighting)}");
                return;
            }

            // light intensity depending on blade size
            _light.intensity = _scaleCurrent;
        }
        

        #endregion

        #region Helpers

        /// <summary>
        /// Get the materials of the mesh renderer.
        /// </summary>
        /// <returns>The materials.</returns>
        private Material[] GetMaterials()
        {
            return !Application.isPlaying && Application.isEditor ? MeshRenderer.sharedMaterials : MeshRenderer.materials;
        }

        #endregion
    }
}
