namespace SND.SMP.Configuration.Ui
{
    public class UiThemeInfo(string name, string cssClass)
    {
        public string Name { get; } = name;
        public string CssClass { get; } = cssClass;
    }
}
