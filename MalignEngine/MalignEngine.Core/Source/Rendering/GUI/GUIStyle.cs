namespace MalignEngine
{
    public class GUIStyle
    {
        public static GUIStyle Default { get; set;}

        public Texture2D FrameTexture;
        public Font NormalFont;

        public Color ButtonDefaultColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color ButtonHoverColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        public Color ButtonClickColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        static GUIStyle()
        {
            Default = new GUIStyle()
            {
                FrameTexture = Texture2D.White
            };
        }
    }
}