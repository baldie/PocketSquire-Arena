using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PocketSquire.Arena.Unity.UI.LevelUp
{
    public class AttributeRow : MonoBehaviour
    {
        public string AttributeKey { get; private set; }

        private Button _plusButton;
        private Button _minusButton;
        private TextMeshProUGUI _valueText;

        public Button PlusButton => _plusButton;
        public Button MinusButton => _minusButton;

        public void Initialize()
        {
            // The AttributeKey is derived from the GameObject name (e.g., "STR")
            AttributeKey = gameObject.name;

            // Find child components by name as per requirements
            var plusTransform = transform.Find("plus");
            if (plusTransform != null) _plusButton = plusTransform.GetComponent<Button>();

            var minusTransform = transform.Find("minus");
            if (minusTransform != null) _minusButton = minusTransform.GetComponent<Button>();

            var attrTransform = transform.Find("attr");
            if (attrTransform != null) _valueText = attrTransform.GetComponent<TextMeshProUGUI>();

            if (_plusButton == null || _minusButton == null || _valueText == null)
            {
                Debug.LogError($"[AttributeRow] Missing required children on {gameObject.name}. ensure 'plus' (Button), 'minus' (Button), and 'attr' (TextMeshPro) exist.");
            }
        }

        public void UpdateView(int value, bool canIncrement, bool canDecrement)
        {
            if (_valueText != null)
            {
                _valueText.text = value.ToString();
            }

            if (_plusButton != null)
            {
                _plusButton.interactable = canIncrement;
            }

            if (_minusButton != null)
            {
                _minusButton.interactable = canDecrement;
            }
        }
    }
}
