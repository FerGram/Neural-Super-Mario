using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataGatherer : MonoBehaviour
{
    private float[] _data;

    private bool _gatheringData = false;

    private int _runCount = 0;
    private float _jumpCount = 0;
    private float _jumpEndCount = 0;
    private float _leftCount = 0;
    private float _leftEndCount = 0;
    private float _rightCount = 0;
    private float _rightEndCount = 0;
    private float _runTime = 0;

    private PlayerMover _playerMover;
    private PlayerHealth _playerHealth;

    public float[] GetData() => _data;

    private void OnEnable()
    {
        _playerMover = GetComponent<PlayerMover>();
        _playerMover.OnJumpStarted += () => { if (_gatheringData) _jumpCount++; };
        _playerMover.OnJumpEnded += () => { if (_gatheringData) _jumpEndCount++; };

        _playerHealth = GetComponent<PlayerHealth>();
        _playerHealth.OnLiveLost += SaveLiveData;

        InitiateDataGathering();
        Debug.Log("Data gathering initiated");
    }

    private void OnDestroy()
    {
        _playerMover = GetComponent<PlayerMover>();
        _playerMover.OnJumpStarted -= () => { if (_gatheringData) _jumpCount++; };
        _playerMover.OnJumpEnded -= () => { if (_gatheringData) _jumpEndCount++; };

        _playerHealth = GetComponent<PlayerHealth>();
        _playerHealth.OnLiveLost -= SaveLiveData;
    }

    public void InitiateDataGathering()
    {
        _data = new float[26];

        _gatheringData = true;
    }

    public void StopDataGathering()
    {
        _runCount = 0;
        _gatheringData = false;
    }

    private void Update()
    {
        if (_gatheringData)
        {
            if (Input.GetKeyDown(KeyCode.A)) _leftCount++;
            if (Input.GetKeyUp(KeyCode.A)) _leftEndCount++;
            if (Input.GetKeyDown(KeyCode.D)) _rightCount++;
            if (Input.GetKeyUp(KeyCode.D)) _rightEndCount++;

            _runTime += Time.deltaTime;
        }
    }

    private void SaveLiveData()
    {
        _runTime = Mathf.Round(_runTime);

        _data[5 + 7 * _runCount] = _jumpCount;
        _data[5 + 7 * _runCount + 1] = _leftCount;
        _data[5 + 7 * _runCount + 2] = _rightCount;
        _data[5 + 7 * _runCount + 3] = _jumpEndCount - 1; //Due to initial fall
        _data[5 + 7 * _runCount + 4] = _leftEndCount;   
        _data[5 + 7 * _runCount + 5] = _rightEndCount;
        _data[5 + 7 * _runCount + 6] = _runTime;

        Debug.Log($"Run {_runCount}: {_jumpCount}, {_leftCount}, {_rightCount}, {(_jumpEndCount - 1)}, {_leftEndCount}, {_rightEndCount}, {_runTime}");

        _jumpCount = 0;
        _leftCount = 0;
        _rightCount = 0;
        _jumpEndCount = 0;
        _leftEndCount = 0;
        _rightEndCount = 0;
        _runTime = 0;

        _runCount++;

        if (_runCount >= 3)
        { 
            StopDataGathering();
        }
    }
}