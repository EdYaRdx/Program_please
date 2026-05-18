using EntryPoint;
using UnityEngine;

public class Program
{
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Main()
    {
        UnityApplicationBuilder builder = new UnityApplicationBuilder();

        UnityApplication app = builder.Build();
        app.Run();
    }
}
