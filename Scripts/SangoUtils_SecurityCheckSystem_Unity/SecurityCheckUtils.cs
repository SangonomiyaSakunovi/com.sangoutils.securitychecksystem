using SangoUtils.Converters;
using System;

namespace SangoUtils.SecurityCheckSystem_Unity
{
    internal static class SecurityCheckUtils
    {
        private static bool CheckSignDataValid(string rawData, string signData, SecurityCheckServiceConfig config, SecuritySignConvertProtocol signConvertProtocol)
        {
            bool res = false;
            switch (config.SignMethodCode)
            {
                case SignMethodCode.Md5:
                    res = Md5SignatureUtils.CheckMd5SignDataValid(rawData, signData, config.SecretTimestamp, config.ApiKey, config.ApiSecret, config.CheckLength, signConvertProtocol);
                    break;
            }
            return res;
        }

        private static bool CheckSignDataValid(long rawData, string signData, SecurityCheckServiceConfig config, SecuritySignConvertProtocol signConvertProtocol)
        {
            return CheckSignDataValid(rawData.ToString(), signData, config, signConvertProtocol);
        }

        internal static void CheckProtocl_SIGNDATA(string registLimitTimestampNew, string signData, SecurityCheckServiceConfig config, Action<string> writeRegistInfoCallBack)
        {
            if (CheckSignDataValid(registLimitTimestampNew, signData, config, SecuritySignConvertProtocol.RawData))
            {
                writeRegistInfoCallBack?.Invoke(registLimitTimestampNew);
            }
            else
            {
                config.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateFailed_SignError, "");
            }
        }

        internal static void CheckProtocol_A_B_C_SIGN(string mixSignData, SecurityCheckServiceConfig config, Action<string> writeRegistInfoCallBack)
        {
            if (mixSignData.Length != 3 + config.CheckLength)
            {
                config.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateError_LenghthError, "");
                return;
            }
            int numYearPostNum = NumberUtilsSango.GetNumberFromNumberConvertProtocol(mixSignData[0], NumberConvertProtocol.ASCII_A0a26);
            if (numYearPostNum == -1)
            {
                config.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateError_LenghthError, "");
                return;
            }
            int numYear = 2023 + numYearPostNum;
            int numMonth = NumberUtilsSango.GetNumberFromNumberConvertProtocol(mixSignData[1], NumberConvertProtocol.ASCII_A0a26);
            int numDay = NumberUtilsSango.GetNumberFromNumberConvertProtocol(mixSignData[2], NumberConvertProtocol.ASCII_A0a26);
            DateTime newRegistLimitDateTime = DateTimeUtils.ToDataTime(numYear, numMonth, numDay);
            if (newRegistLimitDateTime == DateTime.MinValue)
            {
                config.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateError_SyntexError, "");
                return;
            }
            string md5DataStr = mixSignData.Substring(3, config.CheckLength);
            long registLimitTimestampNew = newRegistLimitDateTime.ToUnixTimestamp();
            if (CheckSignDataValid(registLimitTimestampNew, md5DataStr, config, SecuritySignConvertProtocol.AllToUpperChar))
            {
                writeRegistInfoCallBack?.Invoke(registLimitTimestampNew.ToString());
            }
            else
            {
                config.OnCheckedResult?.Invoke(RegistInfoCheckResult.UpdateFailed_SignError, "");
            }
        }
    }

    public enum RegistMixSignDataProtocol
    {
        /// <summary>
        /// Custom decode protocol.
        /// </summary>
        SIGN,
        /// <summary>
        /// Default decode protocol, the first 3 sysmbols are the date of the registration limit, and the rest are the sign data, such as MD5.
        /// </summary>
        A_B_C_SIGN
    }
}