public class WaveParams
{
    private int _min, _max;
    private int _scytheDamage;
    private float _playerSpeed;

    public WaveParams(int min, int max, int scytheDamage, float playerSpeed)
    {
        _min = min;
        _max = max;
        _scytheDamage = scytheDamage;
        _playerSpeed = playerSpeed;
    }

    public float GetPlayerSpeed()
    {
        return _playerSpeed;
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