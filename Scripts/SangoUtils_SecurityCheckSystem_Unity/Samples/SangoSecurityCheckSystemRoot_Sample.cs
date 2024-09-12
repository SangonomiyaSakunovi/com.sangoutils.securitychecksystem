using System;
using UnityEngine;

namespace SangoUtils.SecurityCheckSystem_Unity.Samples
{
    public class SangoSecurityCheckSystemRoot_Sample : MonoBehaviour
    {
        /// <summary>
        /// This field should define the Instance, you can get it from the Prefab.
        /// Each prefab such as SangoSecurityCheckRoot, SangoSecurityCheckRoot_4kH, etc can get this scripts.
        /// Pull a prefab on the Hierarchy, then you can get the Instance.
        /// </summary>
        [SerializeField]
        private SangoSecurityCheckRoot _sangoSecurityCheckRoot;
        /// <summary>
        /// This is the sample behavior.
        /// When the SecurityCheck is Valid, this cover will be hide.
        /// You can use a full image cover which on Hierarchy, to see what happened.
        /// </summary>
        [SerializeField]
        private GameObject _cover;

        private void Start()
        {
            InitSecurity();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                ClearRegistInfo();
            }
        }

        /// <summary>
        /// You must call the API: SetConfigs before you call OnInit.
        /// </summary>
        private void InitSecurity()
        {
            _sangoSecurityCheckRoot.SetConfigs(SecurityCheckServiceInfoConfig, GameStart);
            _sangoSecurityCheckRoot.OnInit();
        }

        /// <summary>
        /// The Callback function when the SecurityCheck is Valid.
        /// </summary>
        private void GameStart()
        {
            _cover.SetActive(false);
        }

        /// <summary>
        /// In this config, if you do not know what is OnCheckedResult do, you should set it as null, will use the defalt event.
        /// You also can custom this event by your own behavior.
        /// </summary>
        private SecurityCheckServiceConfig SecurityCheckServiceInfoConfig = new()
        {
            ApiKey = "s",
            ApiSecret = "s",
            SecretTimestamp = "0",
            DefaultRegistLimitDateTime = new DateTime(2022, 2, 22, 0, 0, 0),
            KeyboardResolutionCode = 1,
            RegistInfoCode = RegistInfoCode.Timestamp,
            SignMethodCode = SignMethodCode.Md5,
            CheckLength = 5,
            RegistMixSignDataProtocol = RegistMixSignDataProtocol.A_B_C_SIGN,
            OnCheckedResult = null
        };

        /// <summary>
        /// You can clear the RegistInfo under the Test Mode.
        /// </summary>
        private void ClearRegistInfo()
        {
            _sangoSecurityCheckRoot.ClearRegistInfo();
        }
    }
}