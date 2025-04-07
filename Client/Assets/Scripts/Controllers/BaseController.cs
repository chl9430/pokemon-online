using UnityEngine;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }

    void Start()
    {
        Init();
    }

    void Update()
    {
    }

    protected virtual void Init()
    {

    }
}