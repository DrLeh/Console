namespace DLeh.Console.Clipboard
{
    public interface IClipboardManager
    {
        void Copy(string s);
        string Paste();
    }
}
