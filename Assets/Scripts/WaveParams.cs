public class WaveParams
{
    private int _min, _max;
    private int _scytheDamage;

    public WaveParams(int min, int max, int scytheDamage)
    {
        _min = min;
        _max = max;
        _scytheDamage = scytheDamage;
    }

    public int GetScytheDamage()
    {
        return _scytheDamage;
    }

    public int GetMin()
    {
        return _min;
    }

    public int GetMax()
    {
        return _max;
    }
}