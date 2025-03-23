using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Numerics;

namespace MalignEngine
{
    public interface IDrawImGui : ISchedule
    {
        void OnDrawImGui(float deltaTime);
    }

    public class ImGuiSystem : IService, IPostDrawGUI
    {
        private ScheduleManager scheduleManager;

        private ImGuiController imGuiController;

        public ImGuiSystem(WindowService windowService, GLRenderingAPI glRenderingAPI, InputSystem inputSystem, ScheduleManager scheduleManager)
        {
            this.scheduleManager = scheduleManager;

            imGuiController = new ImGuiController(glRenderingAPI.gl, windowService.window, inputSystem.input, () =>
            {
                var io = ImGuiNET.ImGui.GetIO();
                //io.Fonts.AddFontFromFileTTF("Content/fonts/Ruda-Regular.ttf", 18f);
                io.ConfigFlags |= ImGuiNET.ImGuiConfigFlags.ViewportsEnable;
                io.ConfigFlags |= ImGuiNET.ImGuiConfigFlags.DockingEnable;
                io.ConfigWindowsMoveFromTitleBarOnly = true;

                io.DeltaTime = 1f / 120f;

                ImGuiStylePtr style = ImGui.GetStyle();
                var colors = style.Colors;
                colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
                colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
                colors[(int)ImGuiCol.WindowBg] = new Vector4(0.06f, 0.06f, 0.06f, 0.94f);
                colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
                colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
                colors[(int)ImGuiCol.Border] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f);
                colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
                colors[(int)ImGuiCol.FrameBg] = new Vector4(0.44f, 0.44f, 0.44f, 0.60f);
                colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.57f, 0.57f, 0.57f, 0.70f);
                colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.76f, 0.76f, 0.76f, 0.80f);
                colors[(int)ImGuiCol.TitleBg] = new Vector4(0.04f, 0.04f, 0.04f, 1.00f);
                colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.16f, 0.16f, 0.16f, 1.00f);
                colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.60f);
                colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
                colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
                colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
                colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
                colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
                colors[(int)ImGuiCol.CheckMark] = new Vector4(0.13f, 0.75f, 0.55f, 0.80f);
                colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.13f, 0.75f, 0.75f, 0.80f);
                colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.13f, 0.75f, 1.00f, 0.80f);
                colors[(int)ImGuiCol.Button] = new Vector4(0.13f, 0.75f, 0.55f, 0.40f);
                colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.13f, 0.75f, 0.75f, 0.60f);
                colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.13f, 0.75f, 1.00f, 0.80f);
                colors[(int)ImGuiCol.Header] = new Vector4(0.13f, 0.75f, 0.55f, 0.40f);
                colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.13f, 0.75f, 0.75f, 0.60f);
                colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.13f, 0.75f, 1.00f, 0.80f);
                colors[(int)ImGuiCol.Separator] = new Vector4(0.13f, 0.75f, 0.55f, 0.40f);
                colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.13f, 0.75f, 0.75f, 0.60f);
                colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.13f, 0.75f, 1.00f, 0.80f);
                colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.13f, 0.75f, 0.55f, 0.40f);
                colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.13f, 0.75f, 0.75f, 0.60f);
                colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.13f, 0.75f, 1.00f, 0.80f);
                colors[(int)ImGuiCol.Tab] = new Vector4(0.13f, 0.75f, 0.55f, 0.80f);
                colors[(int)ImGuiCol.TabHovered] = new Vector4(0.13f, 0.75f, 0.75f, 0.80f);
                colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.13f, 0.75f, 0.55f, 0.80f);
                colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.13f, 0.13f, 0.13f, 0.80f);
                colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
                colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
                colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
                colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
                colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
                colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
                colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
                colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
                colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.07f);
                colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
                colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
                colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
                colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
                colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
                colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
            });
        }

        public void OnPostDrawGUI(float deltaTime)
        {
            imGuiController.Update(deltaTime);
            scheduleManager.Run<IDrawImGui>(e => e.OnDrawImGui(deltaTime));
            imGuiController.Render();
        }

        public void Image(ITexture texture, Vector2 size, Vector2 uv0, Vector2 uv1)
        {
            ImGui.Image((IntPtr)((GLTextureHandle)texture.Handle).textureHandle, size, uv0, uv1);
        }

        public void Image(ITexture texture, Vector2 size)
        {
            ImGui.Image((IntPtr)((GLTextureHandle)texture.Handle).textureHandle, size);
        }

        public bool ImageButton(string id, ITexture texture, Vector2 size)
        {
            return ImGui.ImageButton(id, (IntPtr)((GLTextureHandle)texture.Handle).textureHandle, size);
        }

        public bool ImageButton(string id, ITexture texture, Vector2 size, Vector2 uv1, Vector2 uv2)
        {
            return ImGui.ImageButton(id, (IntPtr)((GLTextureHandle)texture.Handle).textureHandle, size, uv1, uv2);
        }
    }
}