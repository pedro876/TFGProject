namespace FuncSpace
{
    public interface IFuncEncoder
    {
        Bytecode Encode(IFuncNode rootNode);
    }
}