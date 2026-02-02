public interface IEncryptionManager
{
    void BeginSession();
    void EndSession();
    bool VerifyKey();
    string EncryptString(string plain);
    string DecryptString(string cipher);
    void SaveEncryptedToFile(string fileName, string plainText);
    string LoadDecryptedFromFile(string fileName);
}

