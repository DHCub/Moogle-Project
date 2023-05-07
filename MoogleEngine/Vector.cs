namespace MoogleEngine;
using System.Text.Json.Serialization;
public class Vector<T> where T : notnull
{
    // this field must be made public in order to be serialized into a json file in the caching process
    // an underscore is prepended to denote it must not be used
    public Dictionary<T, double> _dict;
    
    public Vector(Dictionary<T, double> InputDict, bool SAFE = false) 
    {
        if (InputDict == null) 
        {
            this._dict = new Dictionary<T, double>();
            return;
        }
        
        if (SAFE) this._dict = InputDict;
        else
        {
            this._dict = new Dictionary<T, double>();
            foreach(var dimension in InputDict.Keys)
            {
                this._dict[dimension] = InputDict[dimension];
            }
        }
    }

    public Vector(Vector<T> other) : this(other._dict) {}

    public Vector()
    {
        _dict = new Dictionary<T, double>();
    }

    [JsonIgnore]
    public Dictionary<T, double>.KeyCollection Dimensions
    {
        get
        {
            return this._dict.Keys;
        }
    }

    public bool Dimension_Not_0(T dimension)
    {
        return this._dict.ContainsKey(dimension);
    }
    public bool Dimension_Is_0(T dimension) => !Dimension_Not_0(dimension);

    public double this[T dimension]
    {
        get
        {
            if (this.Dimension_Not_0(dimension)) return this._dict[dimension];
            else return 0;
        }
        set
        {
            if (value != 0) this._dict[dimension] = value;
            else this._dict.Remove(dimension);
        }
    }

    public double DotProduct(Vector<T> other)
    {
        double answ = 0;
        foreach(var dimension in this.Dimensions)
        {
            if (other.Dimension_Not_0(dimension))
                answ += this[dimension]*other[dimension];
        }

        return answ;
    }

    public double Cosine(Vector<T> other)
    {
        return this.DotProduct(other)/(this.Norm()*other.Norm());
    }

    public double Norm()
    {
        double answ = 0;

        foreach(var dimension in this._dict.Keys)
        {
            answ += Math.Pow(this[dimension], 2);
        }

        return Math.Pow(answ, 0.5);
    }
}

