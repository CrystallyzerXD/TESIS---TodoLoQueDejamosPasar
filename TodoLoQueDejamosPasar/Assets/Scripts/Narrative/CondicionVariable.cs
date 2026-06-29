using UnityEngine;

public enum TipoCondicion
{
    Ignorar,       // no aplica esta condicion
    MayorQue,      // variable > valor
    MayorOIgual,   // variable >= valor
    Igual,         // variable == valor
    MenorOIgual,   // variable <= valor
    MenorQue       // variable < valor
}

[System.Serializable]
public class CondicionVariable
{
    public TipoCondicion tipo  = TipoCondicion.Ignorar;
    public int           valor = 5;

    public bool Cumplida(int valorActual)
    {
        switch (tipo)
        {
            case TipoCondicion.Ignorar:      return true;
            case TipoCondicion.MayorQue:     return valorActual >  valor;
            case TipoCondicion.MayorOIgual:  return valorActual >= valor;
            case TipoCondicion.Igual:        return valorActual == valor;
            case TipoCondicion.MenorOIgual:  return valorActual <= valor;
            case TipoCondicion.MenorQue:     return valorActual <  valor;
            default:                         return true;
        }
    }
}