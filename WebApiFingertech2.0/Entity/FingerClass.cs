using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.IO;
using NBioBSPCOMLib;
using NITGEN.SDK.NBioBSP;

namespace WebApiFingertech2._0.Entity
{
    public static class FingerClass
    {
        public static NBioBSPCOMLib.NBioBSP objNBioBSP;
        public static NBioBSPCOMLib.IDevice objDevice;
        public static NBioBSPCOMLib.IExtraction objExtraction;
        public static NBioBSPCOMLib.IMatching objMatching;
        public static NBioBSPCOMLib.IFPData objFPData;
        public static NBioBSPCOMLib.IFPImage objFPImage;
        static byte[] rawFile;
        static byte[] wsqFile;
        static readonly string dir = "C:\\Users\\Suporte4\\Desktop\\WebApiFingertech2.0\\WebApiFingertech2.0\\img\\";//ESCOLHER DIRETORIO PARA SALVAR IMAGEM
        public static string Capture(int id)
        {
            NBioAPI m_NBioAPI = new NBioAPI();
            NBioAPI.Type.INIT_INFO_0 initInfo0;
            uint ret = m_NBioAPI.GetInitInfo(out initInfo0);
            if (ret == NBioAPI.Error.NONE)
            {
                initInfo0.EnrollImageQuality = Convert.ToUInt32(50);
                initInfo0.VerifyImageQuality = Convert.ToUInt32(30);
                initInfo0.DefaultTimeout = Convert.ToUInt32(10000);
                initInfo0.SecurityLevel = (int)NBioAPI.Type.FIR_SECURITY_LEVEL.NORMAL - 1;
            }

            NBioAPI.IndexSearch m_IndexSearch = new NBioAPI.IndexSearch(m_NBioAPI);
            NBioAPI.Type.HFIR hCapturedFIR;
            NBioAPI.Type.FIR_TEXTENCODE m_textFIR;
            // Get FIR data
            m_NBioAPI.OpenDevice(NBioAPI.Type.DEVICE_ID.AUTO);
            m_NBioAPI.Capture(out hCapturedFIR);

            try
            {
                if (hCapturedFIR != null)
                {
                    m_NBioAPI.GetTextFIRFromHandle(hCapturedFIR, out m_textFIR, true);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(new { user_id = id, textFir = m_textFIR.TextFIR.ToString() });

                }
                return null;
            }
            catch (Exception ex)
            {

                throw new Exception("ERRO:... " + ex.Message);
            }
        }

        public static string Enroll(int id)
        {
            NBioAPI m_NBioAPI = new NBioAPI();
            NBioAPI.Type.FIR_TEXTENCODE m_textFIR;
            NBioAPI.Type.HFIR NewFIR;
            NBioAPI.IndexSearch m_IndexSearch = new NBioAPI.IndexSearch(m_NBioAPI);


            NBioAPI.Type.WINDOW_OPTION m_WinOption = new NBioAPI.Type.WINDOW_OPTION();
            m_WinOption.WindowStyle = (uint)NBioAPI.Type.WINDOW_STYLE.NO_WELCOME;

            string Retorno = "";

            m_NBioAPI.OpenDevice(NBioAPI.Type.DEVICE_ID.AUTO);
            uint ret = m_NBioAPI.Enroll(out NewFIR, null);

            //uint ret = m_NBioAPI.Enroll(null, out NewFIR, null, NBioAPI.Type.TIMEOUT.DEFAULT, null, m_WinOption);


            if (ret != NBioAPI.Error.NONE)
            {
                m_NBioAPI.CloseDevice(NBioAPI.Type.DEVICE_ID.AUTO);
            }

            if (NewFIR != null)
            {
                m_NBioAPI.GetTextFIRFromHandle(NewFIR, out m_textFIR, true);

                if (m_textFIR.TextFIR != null)
                {
                    m_NBioAPI.CloseDevice(NBioAPI.Type.DEVICE_ID.AUTO);
                    Retorno = Newtonsoft.Json.JsonConvert.SerializeObject(new { user_id = id, textFir = m_textFIR.TextFIR.ToString() });
                }
            }
            return Retorno;
        }

        public static string Identificar(String digital)
        {
            NBioAPI m_NBioAPI = new NBioAPI();
            NBioAPI.Type.FIR_TEXTENCODE m_textFIR = new NBioAPI.Type.FIR_TEXTENCODE();
            //NBioAPI.Type.HFIR NewFIR;
            NBioAPI.IndexSearch m_IndexSearch = new NBioAPI.IndexSearch(m_NBioAPI);
            NBioAPI.Type.HFIR hCapturedFIR;
            NBioAPI.IndexSearch.FP_INFO[] fpInfo;


            NBioAPI.Type.WINDOW_OPTION m_WinOption = new NBioAPI.Type.WINDOW_OPTION();
            m_WinOption.WindowStyle = (uint)NBioAPI.Type.WINDOW_STYLE.NO_WELCOME;

            uint ID = 1;

            m_textFIR.TextFIR = digital;
            m_IndexSearch.AddFIR(m_textFIR, ID, out fpInfo);

            uint dataCount;
            m_IndexSearch.GetDataCount(out dataCount);

            m_NBioAPI.OpenDevice(NBioAPI.Type.DEVICE_ID.AUTO);
            uint ret = m_NBioAPI.Capture(out hCapturedFIR);

            if (ret != NBioAPI.Error.NONE)
            {
                //DisplayErrorMsg(ret);
                m_NBioAPI.CloseDevice(NBioAPI.Type.DEVICE_ID.AUTO);
                m_NBioAPI.GetTextFIRFromHandle(hCapturedFIR, out m_textFIR, true);
            }

            m_NBioAPI.CloseDevice(NBioAPI.Type.DEVICE_ID.AUTO);

            NBioAPI.IndexSearch.FP_INFO fpInfo2;
            NBioAPI.IndexSearch.CALLBACK_INFO_0 cbInfo0 = new NBioAPI.IndexSearch.CALLBACK_INFO_0();
            cbInfo0.CallBackFunction = new NBioAPI.IndexSearch.INDEXSEARCH_CALLBACK(myCallback);

            // Identify FIR to IndexSearch DB
            ret = m_IndexSearch.IdentifyData(hCapturedFIR, NBioAPI.Type.FIR_SECURITY_LEVEL.NORMAL, out fpInfo2, cbInfo0);
            if (ret != NBioAPI.Error.NONE)
            {
                //DisplayErrorMsg(ret);
                return fpInfo2.ID.ToString();

            }

            return "";

            uint myCallback(ref NBioAPI.IndexSearch.CALLBACK_PARAM_0 cbParam0, IntPtr userParam)
            {
                //progressIdentify.Value = Convert.ToInt32(cbParam0.ProgressPos);
                return NBioAPI.IndexSearch.CALLBACK_RETURN.OK;

            }
        }

        public static string Comparar(String digital)
        {
            uint ret;
            bool result;
            NBioAPI m_NBioAPI = new NBioAPI();
            NBioAPI.Type.HFIR hCapturedFIR = new NBioAPI.Type.HFIR();
            NBioAPI.Type.FIR_TEXTENCODE m_textFIR = new NBioAPI.Type.FIR_TEXTENCODE();
            NBioAPI.Type.FIR_PAYLOAD myPayload = new NBioAPI.Type.FIR_PAYLOAD();

            m_textFIR.TextFIR = digital.ToString();

            m_NBioAPI.OpenDevice(NBioAPI.Type.DEVICE_ID.AUTO);
            m_NBioAPI.Capture(out hCapturedFIR);

            ret = m_NBioAPI.VerifyMatch(hCapturedFIR, m_textFIR, out result, myPayload);

            if (result == true)
                return "OK";
            else
                return "";
        }

        public static string Converter()
        {
            // Create NBioBSP object
            NBioBSPClass objNBioBSP = new NBioBSPClass();
            objDevice = (NBioBSPCOMLib.IDevice)objNBioBSP.Device;
            objExtraction = (NBioBSPCOMLib.IExtraction)objNBioBSP.Extraction;
            objMatching = (NBioBSPCOMLib.IMatching)objNBioBSP.Matching;
            objFPData = (NBioBSPCOMLib.IFPData)objNBioBSP.FPData;
            objFPImage = (NBioBSPCOMLib.IFPImage)objNBioBSP.FPImage;
            objDevice.Open(255);

            objExtraction.Capture();
            objFPImage.Export();

            objDevice.Close(255);

            rawFile = (byte[])objFPImage.get_RawData(0, 0);
            string path = SaveImageFile(dir + "fingerprint.raw", rawFile);

            if (rawFile != null)
            {
                ConverterToRaw();
                return "Imagem raw e wsq gerada: " + path;
            }
            return "Erro";
        }
        private static string SaveImageFile(string strFilePath, byte[] ImageData)
        {
            FileStream sFile = new FileStream(strFilePath, FileMode.Create, FileAccess.ReadWrite);

            sFile.Seek(0, SeekOrigin.Begin);
            sFile.Write(ImageData, 0, ImageData.Length);
            sFile.Close();
            return strFilePath;
        }
        private static void ConverterToRaw()
        {
            if (wsqFile != null)
                wsqFile = null;

            wsqFile = (byte[])objFPImage.ConvertRawToWsq(objFPImage.ImageWidth, objFPImage.ImageHeight, (object)rawFile, (float)7.0);
            SaveImageFile(dir + "fingerprint.wsq", wsqFile);
        }
    }
}
