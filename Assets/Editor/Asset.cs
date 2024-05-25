using UnityEditor;


namespace UnusedAssetDeleter
{
    public class Asset
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string Guid { get; private set; }

        public Asset(string name, string path, string guid)
        {
            Name = name;
            Path = path;
            Guid = guid;
        }
        public static Asset CreateByPath(string path)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            var name = AssetDatabase.LoadMainAssetAtPath(path).name;
            return new Asset(name, path, guid);
        }

        public static Asset CreateByGuid(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = AssetDatabase.LoadMainAssetAtPath(path).name;
            return new Asset(name, path, guid);
        }
        public bool Equals(Asset data)
        {
            return Guid == data.Guid;
        }
    }
}

