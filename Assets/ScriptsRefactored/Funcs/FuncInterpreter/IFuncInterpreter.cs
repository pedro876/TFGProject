namespace FuncSpace
{
    public interface IFuncInterpreter
    {
        IFuncNode CreateNodeTreeForFunc(string definition, string funcName);
    }
}