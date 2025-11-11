


/// <summary>
/// A lightweight struct that holds a float min and float max.
/// </summary>
[System.Serializable]
public struct MinMaxFloat
{
    public float min;
    public float max;

    public MinMaxFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }


    // Math oerators
    public static MinMaxFloat operator +(MinMaxFloat a, MinMaxFloat b)
    {
        return new MinMaxFloat(a.min + b.min, a.max + b.max);
    }
    public static MinMaxFloat operator -(MinMaxFloat a, MinMaxFloat b)
    {
        return new MinMaxFloat(a.min - b.min, a.max - b.max);
    }
    public static MinMaxFloat operator *(MinMaxFloat a, MinMaxFloat b)
    {
        return new MinMaxFloat(a.min * b.min, a.max * b.max);
    }
    public static MinMaxFloat operator *(MinMaxFloat a, float b)
    {
        return new MinMaxFloat(a.min * b, a.max * b);
    }
}