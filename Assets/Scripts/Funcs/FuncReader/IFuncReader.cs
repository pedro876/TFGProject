namespace FuncSpace
{
    public interface IFuncReader
    {
        IFunc ExtractOriginalFuncInfo(string textFunc, IFunc func);
        IFunc ExtractFinalFuncInfo(IFunc func);
    }
}