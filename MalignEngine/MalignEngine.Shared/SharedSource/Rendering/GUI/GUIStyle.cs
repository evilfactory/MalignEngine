namespace MalignEngine
{
    public class GUIStyle
    {
        public static GUIStyle Default { get; set;}

        public Texture2D FrameTexture;
        public Font NormalFont;

        static GUIStyle()
        {
            Default = new GUIStyle()
            {
                FrameTexture = Texture2D.White
            };
        }
    }
}