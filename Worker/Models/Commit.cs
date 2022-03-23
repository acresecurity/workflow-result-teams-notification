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
            if (!string.IsNullOrWhiteSpace(value) && value.IndexOf("\n") > 0)
            {
                _message = value.Substring(0, value.IndexOf("\n"));
            }
            else
            {
                _message = value;
            }
        }
    }
    public string UrlLink = "";
}