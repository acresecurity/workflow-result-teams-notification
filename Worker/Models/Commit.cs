namespace Worker.Models;

public class Commit
{
    public string Author = "";
    private string _message = "";
    public string Message
    {
        get
        {
            return _message;
        }
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && value.StartsWith("Merge pull request #") && value.IndexOf("from") > 0)
            {
                _message = value.Substring(0, value.IndexOf("from"));
            }
            else
            {
                _message = value;
            }
        }
    }
    public string UrlLink = "";
}