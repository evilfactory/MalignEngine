namespace MalignEngine.Editor;

public static class Defaults
{
    public static void Editor(Application application, EntityManager entityManager)
    {
        application.ServiceContainer.RegisterAll<ImGuiSystem>();
        application.ServiceContainer.RegisterAll<PropertyEditor>();
        application.ServiceContainer.RegisterAll<DefaultMemberEditors>();
        application.ServiceContainer.RegisterAll<EditorSystem>();
        application.ServiceContainer.RegisterAll<EditorConsole>();
        application.ServiceContainer.RegisterAll<EditorPerformanceSystem>();
        application.ServiceContainer.RegisterAll<EditorAssetViewer>();
        entityManager.WorldContainer.RegisterAll<EditorSceneViewSystem>();
        entityManager.WorldContainer.RegisterAll<EntityInspectorEditor>();
    }
}