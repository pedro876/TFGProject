namespace FuncSpace
{
    public interface IFuncSimplifier
    {
        IFuncNode SimplifyFuncFromRoot(IFuncNode rootNode);
    }
}