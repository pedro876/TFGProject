namespace RenderingSpace
{
    public interface IRendererFactory
    {
        IRenderer CreateCPURenderer(int level = 0, QuadRenderer parent = null);
        IRenderer CreateGPURenderer(int level = 0, QuadRenderer parent = null);
    }
}