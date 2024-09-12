using System;
using UnityEngine;

namespace SangoUtils.SecurityCheckSystem_Unity
{
    public class SangoSecurityCheckRoot : MonoBehaviour
    {
        public Action OnSecurityCheckResultValidCB { get; private set; }
        public SecurityCheckServiceConfig SecurityCheckServiceConfig { get; private set; }

        private SangoSecurityCheckWnd _sangoSecurityCheckWnd;

        private TypeInConfig _currentTypeInConfig;

        private bool _isSecuritySystemInit = false;

        public void SetConfigs(SecurityCheckServiceConfig config, Action onSecurityCheckResultValidCB)
        {
            SecurityCheckServiceConfig = config;
            OnSecurityCheckResultValidCB = onSecurityCheckResultValidCB;
            _isSecuritySystemInit = true;
        }

        public void OnInit()
        {
            _sangoSecurityCheckWnd = transform.Find("SangoSecurityCheckWnd").GetComponent<SangoSecurityCheckWnd>();

            if (!_isSecuritySystemInit)
            {
                Debug.LogError("SangoSecurityCheckSystem Error: You need Call the API: 'SetConfigs' before OnInit.");
                return;
            }

            if (SecurityCheckServiceConfig.OnCheckedResult == null)
            {
                SecurityCheckServiceConfig.OnCheckedResult = RegistInfoCheckResultActionCallBack;
            }

            TypeInConfig typeInConfig = new TypeInConfig();
            switch (SecurityCheckServiceConfig.KeyboardResolutionCode)
            {
                case 1:
                    typeInConfig.keyboardTypeCode = KeyboardTypeCode.UpperCharKeyboard;
                    typeInConfig.keyboradDirectionCode = KeyboradDirectionCode.Horizontal;
                    break;
                case 3:
                    typeInConfig.keyboardTypeCode = KeyboardTypeCode.UpperCharKeyboard_4K;
                    typeInConfig.keyboradDirectionCode = KeyboradDirectionCode.Horizontal;
                    break;
                case 4:
                    typeInConfig.keyboardTypeCode = KeyboardTypeCode.UpperCharKeyboard_Vertical_4K;
                    typeInConfig.keyboradDirectionCode = KeyboradDirectionCode.Vertical;
                    break;
            }
            typeInConfig.typeInLanguage = TypeInLanguage.English;
            _currentTypeInConfig = typeInConfig;
            InitSecurityCheckService(SecurityCheckServiceConfig);
            CheckRegistValidation();
        }

        public void ClearRegistInfo()
        {
            SecurityCheckService.Instance.ClearRegistInfo();
        }

        private void CheckRegistValidation()
        {
            SecurityCheckService.Instance.CheckRegistValidation();
        }

        internal void UpdateRegistInfo(string registLimitTimestampNew, string signData)
        {
            SecurityCheckService.Instance.UpdateRegistInfo(registLimitTimestampNew, signData);
        }

        internal void UpdateRegistInfo(string mixSignData)
        {
            SecurityCheckService.Instance.UpdateRegistInfo(mixSignData);
        }

        private void InitSecurityCheckService(SecurityCheckServiceConfig config)
        {
            SecurityCheckService.Instance.InitService(config);
        }

        private void RegistInfoCheckResultActionCallBack(RegistInfoCheckResult result, string commands)
        {
            switch (result)
            {
                case RegistInfoCheckResult.CheckOK_Valid:
                    OnSecurityCheckResultValid();
                    break;
                case RegistInfoCheckResult.CheckOK_FirstRun:
                    CheckRegistValidation();
                    break;
                case RegistInfoCheckResult.CheckWarnning_ValidationLessThan3Days:
                    _sangoSecurityCheckWnd.SetRoot(this);
                    _sangoSecurityCheckWnd.SetInfo(_currentTypeInConfig);
                    _sangoSecurityCheckWnd.gameObject.SetActive(true);
                    _sangoSecurityCheckWnd.OnInit();
                    _sangoSecurityCheckWnd.UpdateResult("软件还有" + commands + "天到期，请及时联系开发者");
                    _sangoSecurityCheckWnd.UpdateBtnInfo("SkipBtn", "");
                    break;
                case RegistInfoCheckResult.CheckFailed_OutData:
                    _sangoSecurityCheckWnd.SetRoot(this);
                    _sangoSecurityCheckWnd.SetInfo(_currentTypeInConfig);
                    _sangoSecurityCheckWnd.gameObject.SetActive(true);
                    _sangoSecurityCheckWnd.OnInit();
                    _sangoSecurityCheckWnd.UpdateResult("软件已过期，请输入注册码后点击激活");
                    break;
                case RegistInfoCheckResult.CheckError_SystemTimeChanged:
                    _sangoSecurityCheckWnd.SetRoot(this);
                    _sangoSecurityCheckWnd.SetInfo(_currentTypeInConfig);
                    _sangoSecurityCheckWnd.gameObject.SetActive(true);
                    _sangoSecurityCheckWnd.OnInit();
                    _sangoSecurityCheckWnd.UpdateResult("注册信息检查失败，系统时间被修改");
                    break;
                case RegistInfoCheckResult.UpdateOK_Success:
                    _sangoSecurityCheckWnd.UpdateResult("成功激活，软件可运行至" + commands);
                    _sangoSecurityCheckWnd.UpdateBtnInfo("RegistBtn", "确定");
                    break;
                case RegistInfoCheckResult.UpdateFailed_OutData:
                    _sangoSecurityCheckWnd.UpdateResult("激活失败，密钥已过期");
                    break;
                case RegistInfoCheckResult.UpdateFailed_SignError:
                    _sangoSecurityCheckWnd.UpdateResult("输入有误，请重新输入或联系开发者");
                    break;
                case RegistInfoCheckResult.UpdateFailed_WriteInfoError:
                    _sangoSecurityCheckWnd.UpdateResult("写入注册信息失败，请重试");
                    break;
                case RegistInfoCheckResult.UpdateError_NullInfo:
                    _sangoSecurityCheckWnd.UpdateResult("输入有误，输入数据不能为空");
                    break;
                case RegistInfoCheckResult.UpdateError_SyntexError:
                    _sangoSecurityCheckWnd.UpdateResult("输入有误，请重新输入或联系开发者");
                    break;
                case RegistInfoCheckResult.UpdateError_LenghthError:
                    _sangoSecurityCheckWnd.UpdateResult("输入有误，输入的数据长度不正确");
                    break;
            }
        }

        internal void OnSecurityCheckResultValid()
        {
            _sangoSecurityCheckWnd.gameObject.SetActive(false);
            OnSecurityCheckResultValidCB?.Invoke();
        }
    }
}