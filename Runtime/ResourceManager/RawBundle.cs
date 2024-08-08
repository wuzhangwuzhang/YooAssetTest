namespace YooAsset
{
    internal class RawBundle
    {
        private readonly string _filePath;
        private readonly IFileSystem _fileSystem;
        private readonly PackageBundle _packageBundle;

        internal RawBundle(IFileSystem fileSystem, PackageBundle packageBundle, string filePath)
        {
            _fileSystem = fileSystem;
            _packageBundle = packageBundle;
            _filePath = filePath;
        }

        public string GetFilePath()
        {
            return _filePath;
        }

        public byte[] ReadFileData()
        {
            if (_fileSystem != null)
                return _fileSystem.ReadFileData(_packageBundle);
            return FileUtility.ReadAllBytes(_filePath);
        }

        public string ReadFileText()
        {
            if (_fileSystem != null)
                return _fileSystem.ReadFileText(_packageBundle);
            return FileUtility.ReadAllText(_filePath);
        }
    }
}