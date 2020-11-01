public class WaveParams
{
    private int _min, _max;
    private int _scytheDamage;
    private float _playerSpeed;
    private int _superSkeletons;
    private string _message;
    public WaveParams(int min, int max, int scytheDamage, float playerSpeed, string message = "", int superSkeletons = 0)
    {
        _min = min;
        _max = max;
        _scytheDamage = scytheDamage;
        _playerSpeed = playerSpeed;
        _superSkeletons = superSkeletons;
        _message = message;
    }

    public string GetMessage()
    {
        return _message;
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