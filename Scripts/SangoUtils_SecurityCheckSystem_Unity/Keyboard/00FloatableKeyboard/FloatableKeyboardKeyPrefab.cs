using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SangoUtils.SecurityCheckSystem_Unity
{
    public class FloatableKeyboardKeyPrefab : MonoBehaviour
    {
        private Button _keyButton;
        private TMP_Text _keyTextMiddle;
        private TMP_Text _keyTextLeftUp;

        private Button _specialKeyButton;

        private string _typeInWord;
        private Action<string> _specialButtonClickedCallBack;

        internal void InitStandardKey(TMP_FontAsset fontAsset)
        {
            _keyButton = GetComponent<Button>();
            _keyTextLeftUp = transform.GetChild(0).GetComponent<TMP_Text>();
            _keyTextMiddle = transform.GetChild(1).GetComponent<TMP_Text>();
            _keyTextLeftUp.font = fontAsset;
            _keyTextMiddle.font = fontAsset;
        }

        internal void InitSpecialKey(TMP_FontAsset fontAsset)
        {
            _specialKeyButton = GetComponent<Button>();
            transform.GetChild(0).GetComponent<TMP_Text>().font = fontAsset;
        }

        internal void UpdateStandardButtonListener(string middleText, string leftUpText, bool isDispose)
        {
            if (!isDispose)
            {
                _keyButton.onClick.RemoveAllListeners();
                _typeInWord = middleText;
                UpdateButtonName(middleText, leftUpText);
                _keyButton.onClick.AddListener(OnStandardButtonClicked);
            }
            else
            {
                _keyButton.onClick.RemoveAllListeners();
            }
        }

        internal void UpdateSpecialButtonListener(Action<string> specialButtonClickedCallBack, bool isDispose)
        {
            if (!isDispose)
            {
                _specialButtonClickedCallBack = specialButtonClickedCallBack;
                _specialKeyButton.onClick.AddListener(delegate { OnSpecialButtonClicked(_specialKeyButton); });
            }
            else
            {
                _specialKeyButton.onClick.RemoveAllListeners();
                _specialButtonClickedCallBack = null;
            }
        }

        private void UpdateButtonName(string middleText, string leftUpText)
        {
            _keyTextMiddle.SetText(middleText);
            _keyTextLeftUp.SetText(leftUpText);
        }

        private void OnStandardButtonClicked()
        {
            TypeInService.Instance.OnTypedInWord(TypeInCommand.TypeIn, _typeInWord);
        }

        internal void OnSpecialButtonClicked(Button button)
        {
            _specialButtonClickedCallBack?.Invoke(button.name);
        }
    }
}