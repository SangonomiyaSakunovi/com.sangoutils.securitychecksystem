using System;
using UnityEngine;

namespace SangoUtils.SecurityCheckSystem_Unity
{
    public class SecurityCheckService : MonoBehaviour
    {
        private static SecurityCheckService _instance = null;

        public static SecurityCheckService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(SecurityCheckService)) as SecurityCheckService;
                    if (_instance == null)
                    {
                        GameObject gameObject = new GameObject("[" + typeof(SecurityCheckService).FullName + "]");
                        _instance = gameObject.AddComponent<SecurityCheckService>();
                        DontDestroyOnLoad(gameObject);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (null != _instance && _instance != this)
            {
                Destroy(gameObject);
            }
        }

        private SecurityCheckServiceConfig _securityCheckServiceConfig = null;
        private PersistDataType _persistDataType = PersistDataType.PlayerPrefs;

        private string _limitTimestampKey = "key1";
        private string _lastRunTimestampKey = "key2";

        private float _currentTickTime = 1;
        private float _maxTickTime = 60;

        private bool _isApplicationRunValid = false;

        internal void InitService(SecurityCheckServiceConfig config)
        {
            if (config != null)
            {
                _securityCheckServiceConfig = config;
            }
        }

        private void Update()
        {
            if (_isApplicationRunValid)
            {
                if (_currentTickTime > 0)
                {
                    _currentTickTime -= Time.deltaTime;
                }
                else
                {
                    _currentTickTime = _maxTickTime;
                    TickUpdateRunTime();
                }
            }
        }

        internal void UpdateRegistInfo(string registLimitTimestampNew, string signData)
        {
            if (string.IsNullOrEmpty(registLimitTimestampNew) || string.IsNullOrEmpty(signData))
            {
                _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateError_NullInfo, "");
                return;
            }
            if (!long.TryParse(registLimitTimestampNew, out long result))
            {
                _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateError_SyntexError, "");
                return;
            }
            switch (_securityCheckServiceConfig.RegistMixSignDataProtocol)
            {
                case RegistMixSignDataProtocol.SIGN:
                    SecurityCheckUtils.CheckProtocl_SIGNDATA(registLimitTimestampNew, signData, _securityCheckServiceConfig, WriteRegistInfo);
                    break;
            }
        }

        internal void UpdateRegistInfo(string mixSignData)
        {
            if (string.IsNullOrEmpty(mixSignData))
            {
                _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateError_NullInfo, "");
                return;
            }
            switch (_securityCheckServiceConfig.RegistMixSignDataProtocol)
            {
                case RegistMixSignDataProtocol.A_B_C_SIGN:
                    SecurityCheckUtils.CheckProtocol_A_B_C_SIGN(mixSignData, _securityCheckServiceConfig, WriteRegistInfo);
                    break;
            }
        }

        private void WriteRegistInfo(string registLimitTimestampNew)
        {
            long nowTimestamp = DateTime.Now.ToUnixTimestamp();
            if (Convert.ToInt64(registLimitTimestampNew) > nowTimestamp)
            {
                string registLimitTimestampDataNew = DateTimeUtils.ToBase64(registLimitTimestampNew);
                string registLastRunTimestampDataNew = DateTimeUtils.ToBase64(nowTimestamp);
                bool res1 = AddPersistData(_limitTimestampKey, registLimitTimestampDataNew);
                bool res2 = AddPersistData(_lastRunTimestampKey, registLastRunTimestampDataNew);
                if (res1 && res2)
                {
                    _isApplicationRunValid = true;
                    DateTime registNewLimitDateTime = DateTimeUtils.FromUnixTimestampString(registLimitTimestampNew);
                    _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateOK_Success, registNewLimitDateTime.ToString("yyyy-MM-dd"));
                }
                else
                {
                    _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateFailed_WriteInfoError, "");
                }
            }
            else
            {
                Debug.LogWarning("RegistFaild, the NewRegistLimitTimestamp should newer than NowTimestamp.");
                _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateFailed_OutData, "");
            }
        }

        internal void CheckRegistValidation()
        {
            long nowTimestamp = DateTime.Now.ToUnixTimestamp();
            long defaultRegistLimitTimestamp = _securityCheckServiceConfig.DefaultRegistLimitDateTime.ToUnixTimestamp();

            string registLimitTimestampData = GetPersistData(_limitTimestampKey);
            string registLastRunTimestampData = GetPersistData(_lastRunTimestampKey);
            Debug.Log("Now is time to Find the RegistInfo, please wait....................................");
            Debug.Log("The RegistLimitTimestampInfo Found: [ " + registLimitTimestampData + " ]");
            Debug.Log("The LastRunTimestampInfo Found: [ " + registLastRunTimestampData + " ]");

            if (string.IsNullOrEmpty(registLimitTimestampData) || string.IsNullOrEmpty(registLastRunTimestampData))
            {
                bool res = false;
                registLimitTimestampData = DateTimeUtils.ToBase64(defaultRegistLimitTimestamp);
                registLastRunTimestampData = DateTimeUtils.ToBase64(nowTimestamp);
                Debug.LogWarning("That`s the First Time open this software, we give the default registLimitTimestamp is: [ " + defaultRegistLimitTimestamp + " ]");
                bool res1 = AddPersistData(_limitTimestampKey, registLimitTimestampData);
                bool res2 = AddPersistData(_lastRunTimestampKey, registLastRunTimestampData);
                if (res1 && res2)
                {
                    res = true;
                    _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.CheckOK_FirstRun, "");
                }
                else
                {
                    _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateFailed_WriteInfoError, "");
                }
                Debug.Log("Is first regist OK? [ " + res + " ]");
            }
            else
            {
                long registLimitTimestamp = Convert.ToInt64(DateTimeUtils.FromBase64ToString(registLimitTimestampData));
                long registLastRunTimestamp = Convert.ToInt64(DateTimeUtils.FromBase64ToString(registLastRunTimestampData));
                Debug.Log("We DeCrypt the RegistInfo, please wait....................................");
                Debug.Log("The RegistLimitTimestamp is: [ " + registLimitTimestamp + " ]");
                Debug.Log("The LastRunTimestamp is: [ " + registLastRunTimestamp + " ]");
                Debug.Log("The NowTimestamp is: [ " + nowTimestamp + " ]");
                if (nowTimestamp < registLastRunTimestamp)
                {
                    Debug.LogError("Error: SystemTime has in Changed");
                    _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.CheckError_SystemTimeChanged, "");
                }
                else
                {
                    if (nowTimestamp < registLimitTimestamp)
                    {
                        long timestampSpan = Math.Abs(registLimitTimestamp - nowTimestamp);
                        int daySpan = Convert.ToInt32(timestampSpan / (24 * 60 * 60));
                        if (daySpan <= 3)
                        {
                            _isApplicationRunValid = true;
                            _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.CheckWarnning_ValidationLessThan3Days, daySpan.ToString());
                        }
                        else
                        {
                            _isApplicationRunValid = true;
                            _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.CheckOK_Valid, "");
                        }
                    }
                    else
                    {
                        _securityCheckServiceConfig.OnCheckedResult?.Invoke(RegistInfoCheckResult.CheckFailed_OutData, "");
                    }
                }
            }
        }

        private void TickUpdateRunTime()
        {
            long nowTimestamp = DateTime.Now.ToUnixTimestamp();
            string registLastRunTimestampData = GetPersistData(_lastRunTimestampKey);
            if (string.IsNullOrEmpty(registLastRunTimestampData))
            {
                return;
            }
            long lastRunTimetamp = Convert.ToInt64(DateTimeUtils.FromBase64ToString(registLastRunTimestampData));
            if (lastRunTimetamp > nowTimestamp)
            {
                return;
            }
            else
            {
                string registLastRunTimestampDataNew = DateTimeUtils.ToBase64(nowTimestamp);
                AddPersistData(_lastRunTimestampKey, registLastRunTimestampDataNew);
            }
        }

        internal void ClearRegistInfo()
        {
            RemovePersistData("key1");
            RemovePersistData("key2");
            Debug.Log("Clear RegistInfo done.");
        }

        private bool AddPersistData(string key, string value)
        {
            bool res = false;
            switch (_persistDataType)
            {
                case PersistDataType.PlayerPrefs:
                    PlayerPrefs.SetString(key, value);
                    if (PlayerPrefs.GetString(key) == value)
                    {
                        res = true;
                    }
                    break;
            }
            return res;
        }

        private string GetPersistData(string key)
        {
            string res = "";
            switch (_persistDataType)
            {
                case PersistDataType.PlayerPrefs:
                    if (PlayerPrefs.HasKey(key))
                    {
                        res = PlayerPrefs.GetString(key);
                    }
                    break;
            }
            return res;
        }

        private bool RemovePersistData(string key)
        {
            bool res = false;
            switch (_persistDataType)
            {
                case PersistDataType.PlayerPrefs:
                    if (PlayerPrefs.HasKey(key))
                    {
                        PlayerPrefs.DeleteKey(key);
                    }
                    res = true;
                    break;
            }
            return res;
        }
    }

    public enum SignMethodCode
    {
        Md5
    }

    public enum RegistInfoCode
    {
        Timestamp
    }

    public enum RegistInfoCheckResult
    {
        CheckOK_Valid,
        CheckOK_FirstRun,
        CheckWarnning_ValidationLessThan3Days,
        CheckFailed_OutData,
        CheckError_SystemTimeChanged,
        UpdateOK_Success,
        UpdateFailed_OutData,
        UpdateFailed_SignError,
        UpdateFailed_WriteInfoError,
        UpdateError_NullInfo,
        UpdateError_SyntexError,
        UpdateError_LenghthError
    }

    internal enum PersistDataType
    {
        PlayerPrefs,
        Registry
    }

    public class SecurityCheckServiceConfig
    {
        /// <summary>
        /// ApiKey, just one character is enough.
        /// </summary>
        public string ApiKey { get; set; } = "s";
        /// <summary>
        /// ApiSecret, you can set it to any string.
        /// </summary>
        public string ApiSecret { get; set; } = "s";
        /// <summary>
        /// SecretTimestamp, also use as SecretKey, the default is 0.
        /// </summary>
        public string SecretTimestamp { get; set; } = "0";
        /// <summary>
        /// This is the default regist limit date time, it will effect when the first time open the software.
        /// </summary>
        public DateTime DefaultRegistLimitDateTime { get; set; } = new DateTime(2022, 2, 22, 0, 0, 0);
        /// <summary>
        /// 1-1920x1080, 2-1080x1920, 3-3840x2160, 4-2160x3840 
        /// </summary>
        public int KeyboardResolutionCode { get; set; } = 1;
        /// <summary>
        /// You can choose one method, the default is MD5.
        /// </summary>
        public SignMethodCode SignMethodCode { get; set; } = SignMethodCode.Md5;
        /// <summary>
        /// We suggest use Timestamp as the info of the registration.
        /// </summary>
        public RegistInfoCode RegistInfoCode { get; set; } = RegistInfoCode.Timestamp;
        /// <summary>
        /// The length of the check data, when you use ABC_SIGN, the length should be 5.
        /// </summary>
        public int CheckLength { get; set; } = 5;
        /// <summary>
        /// We suggest use A_B_C_SIGN as the protocol of the mix sign data.
        /// </summary>
        public RegistMixSignDataProtocol RegistMixSignDataProtocol { get; set; } = RegistMixSignDataProtocol.A_B_C_SIGN;
        /// <summary>
        /// If you do not know what is OnCheckedResult do, you should set it as null, will use the defalt event.
        /// </summary>
        public Action<RegistInfoCheckResult, string> OnCheckedResult { get; set; } = null;
    }
}