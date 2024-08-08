namespace YooAsset.Editor
{
    public class TaskEncryption
    {
        /// <summary>
        ///     加密文件
        /// </summary>
        public void EncryptingBundleFiles(BuildParametersContext buildParametersContext,
            BuildMapContext buildMapContext)
        {
            var encryptionServices = buildParametersContext.Parameters.EncryptionServices;
            if (encryptionServices == null)
                return;

            if (encryptionServices.GetType() == typeof(EncryptionNone))
                return;

            var progressValue = 0;
            var pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var fileInfo = new EncryptFileInfo();
                fileInfo.BundleName = bundleInfo.BundleName;
                fileInfo.FileLoadPath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                var encryptResult = encryptionServices.Encrypt(fileInfo);
                if (encryptResult.Encrypted)
                {
                    var filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
                    FileUtility.WriteAllBytes(filePath, encryptResult.EncryptedData);
                    bundleInfo.EncryptedFilePath = filePath;
                    bundleInfo.Encrypted = true;
                    BuildLogger.Log($"Bundle file encryption complete: {filePath}");
                }
                else
                {
                    bundleInfo.Encrypted = false;
                }

                // 进度条
                EditorTools.DisplayProgressBar("Encrypting bundle", ++progressValue, buildMapContext.Collection.Count);
            }

            EditorTools.ClearProgressBar();
        }
    }
}