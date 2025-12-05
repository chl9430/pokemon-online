using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveryMachine : MonoBehaviour
{
    int _ballAnimFinishCount;
    List<GameObject> _monsterBallInsts;

    [SerializeField] Transform[] _ballSpots;
    [SerializeField] GameObject _monsterBall;

    public List<GameObject> BallInsts { get { return _monsterBallInsts; } }

    public void StartHeal(int ballCount)
    {
        if (_monsterBallInsts == null)
            _monsterBallInsts = new List<GameObject>();

        StartCoroutine(StartAnim(ballCount));
    }

    IEnumerator StartAnim(int ballCount)
    {
        for (int i = 0; i < ballCount; i++)
        {
            _monsterBallInsts.Add(Instantiate(_monsterBall, _ballSpots[i]));
            yield return 0.5f;
        }

        for (int i = 0; i < _monsterBallInsts.Count; i++)
        {
            _monsterBallInsts[i].GetComponent<Animator>().Play("BallHealing_Sparkling");
        }
    }

    public bool CountBallAnimFinsh()
    {
        _ballAnimFinishCount++;

        if (_ballAnimFinishCount == _monsterBallInsts.Count)
        {
            _ballAnimFinishCount = 0;
            return true;
        }
        else
            return false;
    }

    public void DestroyMachineBall()
    {
        foreach (GameObject ball in _monsterBallInsts)
        {
            Destroy(ball);
        }

        _monsterBallInsts.Clear();
    }
}
