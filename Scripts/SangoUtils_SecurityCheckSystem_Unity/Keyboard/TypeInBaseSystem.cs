using UnityEngine;

namespace SangoUtils.SecurityCheckSystem_Unity
{
    internal class TypeInBaseSystem : MonoBehaviour
    {
        public virtual void SetSystem()
        {
        }

        public virtual void ShowKeyboard() { }

        public virtual void HideKeyboard() { }

        public virtual void SetKeyboardDirection(KeyboradDirectionCode directionCode) { }
    }
}
