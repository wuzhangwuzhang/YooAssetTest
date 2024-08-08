﻿using System;
using System.IO;

namespace YooAsset
{
    internal class FileSystemHelper
    {
        /// <summary>
        ///     文件校验
        /// </summary>
        public static EFileVerifyResult FileVerify(string filePath, long fileSize, string fileCRC,
            EFileVerifyLevel verifyLevel)
        {
            try
            {
                if (File.Exists(filePath) == false)
                    return EFileVerifyResult.DataFileNotExisted;

                // 先验证文件大小
                var size = FileUtility.GetFileSize(filePath);
                if (size < fileSize)
                    return EFileVerifyResult.FileNotComplete;
                if (size > fileSize)
                    return EFileVerifyResult.FileOverflow;

                // 再验证文件CRC
                if (verifyLevel == EFileVerifyLevel.High)
                {
                    var crc = HashUtility.FileCRC32Safely(filePath);
                    if (crc == fileCRC)
                        return EFileVerifyResult.Succeed;
                    return EFileVerifyResult.FileCrcError;
                }

                return EFileVerifyResult.Succeed;
            }
            catch (Exception)
            {
                return EFileVerifyResult.Exception;
            }
        }
    }
}