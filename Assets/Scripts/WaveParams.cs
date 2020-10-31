public class WaveParams
{
    private int _min, _max;
    private int _scytheDamage;
    private float _playerSpeed;
    private int _superSkeletons;

    public WaveParams(int min, int max, int scytheDamage, float playerSpeed, int superSkeletons = 0)
    {
        _min = min;
        _max = max;
        _scytheDamage = scytheDamage;
        _playerSpeed = playerSpeed;
        _superSkeletons = superSkeletons;
    }

    public int GetSuperSkeletonCount()
    {
        return _superSkeletons;
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