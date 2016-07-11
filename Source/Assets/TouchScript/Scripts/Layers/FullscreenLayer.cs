/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Layer which gets all input from a camera. Should be used instead of a background object getting all the pointers which come through.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Fullscreen Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_FullscreenLayer.htm")]
    public class FullscreenLayer : TouchLayer
    {
        #region Constants

        /// <summary>
        /// The type of FullscreenLayer.
        /// </summary>
        public enum LayerType
        {
            /// <summary>
            /// Get pointers from main camera.
            /// </summary>
            MainCamera,

            /// <summary>
            /// Get pointers from specific camera.
            /// </summary>
            Camera,

            /// <summary>
            /// Get all pointers on Z=0 plane without a camera.
            /// </summary>
            Global
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Layer type.
        /// </summary>
        public LayerType Type
        {
            get { return type; }
            set
            {
                if (value == type) return;
                type = value;
                updateCamera();
                cacheCameraTransform();
            }
        }

        /// <summary>
        /// Target camera if <see cref="LayerType.Camera"/> or <see cref="LayerType.MainCamera"/> is used.
        /// </summary>
        public Camera Camera
        {
            get { return _camera; }
            set
            {
                if (value == _camera) return;
                _camera = value;
                if (_camera == null) Type = LayerType.Global;
                else Type = LayerType.Camera;
                setName();
            }
        }

        /// <inheritdoc />
        public override Vector3 WorldProjectionNormal
        {
            get
            {
                if (cameraTransform == null) return transform.forward;
                return cameraTransform.forward;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private LayerType type = LayerType.MainCamera;

        [SerializeField]
        private Camera _camera;

        private Transform cameraTransform;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out HitData hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            if (_camera != null)
            {
                if (!_camera.pixelRect.Contains(position)) return LayerHitResult.Miss;
            }

            hit = new HitData(transform, this);
            switch (checkHitFilters(hit))
            {
                case HitTest.ObjectHitResult.Hit:
                    return LayerHitResult.Hit;
                case HitTest.ObjectHitResult.Error:
                    return LayerHitResult.Error;
                default:
                    return LayerHitResult.Miss;
            }
        }

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            updateCamera();
            base.Awake();
            if (!Application.isPlaying) return;

            cacheCameraTransform();
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            if (_camera == null) Name = "Global Fullscreen";
            else Name = "Fullscreen @ " + _camera.name;
        }

        /// <inheritdoc />
        protected override ProjectionParams createProjectionParams()
        {
            if (_camera) return new CameraProjectionParams(_camera);
            return base.createProjectionParams();
        }

        #endregion

        #region Private functions

        private void updateCamera()
        {
            switch (type)
            {
                case LayerType.Global:
                    _camera = null;
                    break;
                case LayerType.MainCamera:
                    _camera = Camera.main;
                    if (_camera == null) Debug.LogError("No Main camera found!");
                    break;
            }
            setName();
        }

        private void cacheCameraTransform()
        {
            if (_camera == null) cameraTransform = null;
            else cameraTransform = _camera.transform;
        }

        #endregion
    }
}