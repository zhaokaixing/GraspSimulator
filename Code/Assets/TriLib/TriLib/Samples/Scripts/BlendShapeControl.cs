#pragma warning disable 649
using UnityEngine;
using UnityEngine.UI;

namespace TriLib
{
    namespace Samples
    {
        public class BlendShapeControl : MonoBehaviour
        {
            /// <summary>
            /// Blend-shape name <see cref="UnityEngine.UI.Text"/> reference.
            /// </summary>
            [SerializeField]
            private Text _text;

            /// <summary>
            /// Blend-shape weight <see cref="UnityEngine.UI.Slider"/> reference.
            /// </summary>
            [SerializeField]
            private Slider _slider;

            /// <summary>
            /// Gets/Sets the related UI component text.
            /// </summary>
            public string Text
            {
                get
                {
                    return _text.text;
                }
                set
                {
                    _text.text = value;
                }
            }

            /// <summary>
            /// <see cref="UnityEngine.SkinnedMeshRenderer"/> assigned to the blend-shape.
            /// </summary>
            public SkinnedMeshRenderer SkinnedMeshRenderer;

            /// <summary>
            /// <see cref="UnityEngine.Animation"/> assigned to the loaded model.
            /// </summary>
            private Animation _animation;

            /// <summary>
            /// Blend-shape index.
            /// </summary>
            public int BlendShapeIndex;

            /// <summary>
            /// Indicates that the slider value has been changed from this script.
            /// </summary>
            private bool _ignoreValueChange;

            /// <summary>
            /// Changes the loaded model blend-shape value accordingly.
            /// </summary>
            /// <param name="value">Blend weight.</param>
            public void OnValueChange(float value)
            {
                if (_ignoreValueChange)
                {
                    _ignoreValueChange = false;
                    return;
                }
                AssetLoaderWindow.Instance.HandleBlendEvent(SkinnedMeshRenderer, BlendShapeIndex, value);
            }

            /// <summary>
            /// Stores a reference to the model <see cref="UnityEngine.Animation"/> component.
            /// </summary>
            private void Start()
            {
                _animation = SkinnedMeshRenderer.GetComponentInParent<Animation>();
            }

            /// <summary>
            /// Handles the slider activation/value change.
            /// </summary>
            private void Update()
            {
                if (_animation == null)
                {
                    return;
                }
                if (_animation.isPlaying)
                {
                    if (_slider.interactable)
                    {
                        _slider.interactable = false;
                    }
                    _ignoreValueChange = true;
                    _slider.value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeIndex);
                }
                else if (!_slider.interactable)
                {
                    _slider.interactable = true;
                }
            }
        }
    }
}
#pragma warning restore 649