namespace EasyWeb.View
{
    public interface ITemplate
    {
        string Render(string name, object data);
    }
}
